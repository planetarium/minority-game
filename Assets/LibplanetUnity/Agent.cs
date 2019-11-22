using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Bencodex.Types;
using Libplanet;
using Libplanet.Crypto;
using Libplanet.Net;
using LibplanetUnity.Action;
using LibplanetUnity.Helper;
using NetMQ;
using UnityEngine;

namespace LibplanetUnity
{
    public class Agent : MonoSingleton<Agent>
    {
        public const string PlayerPrefsKeyOfAgentPrivateKey = "private_key_agent";
#if UNITY_EDITOR
        private const string AgentStoreDirName = "planetarium_dev";
#else
        private const string AgentStoreDirName = "planetarium";
#endif

        private static readonly string CommandLineOptionsJsonPath =
            Path.Combine(Application.streamingAssetsPath, "clo.json");

        private const string PeersFileName = "peers.dat";
        private const string IceServersFileName = "ice_servers.dat";

        private static readonly string DefaultStoragePath =
            Path.Combine(Application.persistentDataPath, AgentStoreDirName);

        private static IEnumerator _miner;
        private static IEnumerator _swarmRunner;
        private static IEnumerator _logger;

        private ConcurrentQueue<System.Action> _actions = new ConcurrentQueue<System.Action>();

        public Address Address => _agent.Address;

        private static _Agent _agent;

        public static void Initialize()
        {
            if (!ReferenceEquals(_agent, null))
            {
                return;
            }

            instance.InitAgent();
        }

        private void InitAgent()
        {
            var options = GetOptions(CommandLineOptionsJsonPath);
            var privateKey = GetPrivateKey(options);
            var peers = GetPeers(options);
            var iceServers = GetIceServers();
            var host = GetHost(options);
            int? port = options.Port;
            var storagePath = options.StoragePath ?? DefaultStoragePath;

            _agent = new _Agent(
                privateKey: privateKey,
                path: storagePath,
                peers: peers,
                iceServers: iceServers,
                host: host,
                port: port
            );
            _miner = options.NoMiner ? null : _agent.CoMiner();

            StartSystemCoroutines(_agent);
            StartNullableCoroutine(_miner);
        }

        public static Options GetOptions(string jsonPath)
        {
            if (File.Exists(jsonPath))
            {
                return JsonUtility.FromJson<Options>(
                    File.ReadAllText(jsonPath)
                );
            }
            else
            {
                return CommnadLineParser.GetCommandLineOptions() ?? new Options();
            }
        }

        public IValue GetState(Address address)
        {
            return _agent.GetState(address);
        }

        public void MakeTransaction(IEnumerable<ActionBase> actions)
        {
            _agent.MakeTransaction(actions);
        }

        public void RunOnMainThread(System.Action action)
        {
            _actions.Enqueue(action);
        }

        private static PrivateKey GetPrivateKey(Options options)
        {
            PrivateKey privateKey;
            var privateKeyHex = options.PrivateKey ?? PlayerPrefs.GetString(PlayerPrefsKeyOfAgentPrivateKey, "");

            if (string.IsNullOrEmpty(privateKeyHex))
            {
                privateKey = new PrivateKey();
                PlayerPrefs.SetString(PlayerPrefsKeyOfAgentPrivateKey, ByteUtil.Hex(privateKey.ByteArray));
            }
            else
            {
                privateKey = new PrivateKey(ByteUtil.ParseHex(privateKeyHex));
            }

            return privateKey;
        }

        private static IEnumerable<Peer> GetPeers(Options options)
        {
            return options.Peers?.Any() ?? false
                ? options.Peers.Select(LoadPeer)
                : LoadConfigLines(PeersFileName).Select(LoadPeer);
        }

        private static IEnumerable<IceServer> GetIceServers()
        {
            return LoadIceServers();
        }

        private static string GetHost(Options options)
        {
            return options.Host;
        }

        private static BoundPeer LoadPeer(string peerInfo)
        {
            string[] tokens = peerInfo.Split(',');
            var pubKey = new PublicKey(ByteUtil.ParseHex(tokens[0]));
            string host = tokens[1];
            int port = int.Parse(tokens[2]);

            return new BoundPeer(pubKey, new DnsEndPoint(host, port), 0);
        }

        private static IEnumerable<string> LoadConfigLines(string fileName)
        {
            string userPath = Path.Combine(
                Application.persistentDataPath,
                fileName
            );
            string content;

            if (File.Exists(userPath))
            {
                content = File.ReadAllText(userPath);
            }
            else
            {
                string assetName = Path.GetFileNameWithoutExtension(fileName);
                content = Resources.Load<TextAsset>($"Config/{assetName}").text;
            }

            foreach (var line in Regex.Split(content, "\n|\r|\r\n"))
            {
                if (!string.IsNullOrEmpty(line.Trim()))
                {
                    yield return line;
                }
            }
        }

        private static IEnumerable<IceServer> LoadIceServers()
        {
            foreach (string line in LoadConfigLines(IceServersFileName))
            {
                var uri = new Uri(line);
                string[] userInfo = uri.UserInfo.Split(':');

                yield return new IceServer(new[] {uri}, userInfo[0], userInfo[1]);
            }
        }

        #region Mono

        protected override void OnDestroy()
        {
            _agent?.Dispose();

            NetMQConfig.Cleanup(false);

            base.OnDestroy();
        }

        #endregion

        private void StartSystemCoroutines(_Agent agent)
        {
            _swarmRunner = agent.CoSwarmRunner();

            StartNullableCoroutine(_swarmRunner);
            StartNullableCoroutine(_logger);
            StartCoroutine(CoProcessActions());
        }

        private Coroutine StartNullableCoroutine(IEnumerator routine)
        {
            return ReferenceEquals(routine, null) ? null : StartCoroutine(routine);
        }

        private IEnumerator CoProcessActions()
        {
            while (true)
            {
                if (_actions.TryDequeue(out System.Action action))
                {
                    action();
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        public static bool WantsToQuit()
        {
            return true;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void RunOnStart()
        {
            Application.wantsToQuit += WantsToQuit;
        }
    }
}
