using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bencodex.Types;
using Libplanet;
using Libplanet.Action;

namespace MinorityGame.Model
{
    public class BoardState
    {
        public class Item
        {
            public readonly decimal Stake;
            public readonly IImmutableSet<Address> Bettors;

            public Item(decimal stake, IImmutableSet<Address> bettors = null)
            {
                Stake = stake;
                Bettors = bettors ?? ImmutableHashSet<Address>.Empty;
            }

            public Item(IValue serialized)
            {
                if (serialized is Bencodex.Types.Dictionary dict)
                {
                    Stake = decimal.Parse((Text) dict["stake"]);
                    var binaries = (Bencodex.Types.List) dict["bettors"];
                    var addresses = binaries.Select(addrBin => new Address((Binary) addrBin));
                    Bettors = addresses.ToImmutableHashSet();
                }
                else
                {
                    throw new ArgumentException("Expected a Bencodex dictionary.", nameof(serialized));
                }
            }

            public Item Bet(Address bettor)
            {
                return Bettors.Contains(bettor)
                    ? this
                    : new Item(Stake, Bettors.Add(bettor));
            }

            public IValue Serialize()
            {
                IEnumerable<IValue> bettors = Bettors.Select(address => (IValue) new Binary(address.ToByteArray()));
                return Bencodex.Types.Dictionary.Empty
                    .Add("stake", Stake.ToString())
                    .Add("bettors", (IValue) new Bencodex.Types.List(bettors));
            }
        }

        public const int ItemCount = 25;

        public static readonly Address Address = new Address("0000000000000000000000000000000000000000");

        public readonly IImmutableList<Item> Items;

        public BoardState(IImmutableList<Item> items = null, IRandom random = null)
        {
            Items = items ??
                    Enumerable.Repeat(0, ItemCount)
                        .Select(_ => new Item(random is IRandom r ? r.Next(1, 100) : 0))
                        .ToImmutableArray();
        }

        public BoardState(IValue serialized)
        {
            if (serialized is Bencodex.Types.List list)
            {
                Items = list.Select(item => new Item(item)).ToImmutableArray();
            }
            else
            {
                throw new ArgumentException("Expected a Bencodex list.", nameof(serialized));
            }
        }

        public BoardState Bet(int itemIndex, Address bettor)
        {
            if (itemIndex < 0 || itemIndex >= ItemCount)
                return this;

            var newItems = Items.SetItem(itemIndex, Items[itemIndex].Bet(bettor));
            return new BoardState(newItems);
        }

        public IValue Serialize()
        {
            return new Bencodex.Types.List(Items.Select(item => item.Serialize()));
        }
    }
}
