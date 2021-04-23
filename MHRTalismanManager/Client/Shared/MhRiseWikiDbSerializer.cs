using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MHRTalismanManager.Shared;
using MoreLinq;

namespace MHRTalismanManager.Client.Shared
{
    public static class MhRiseWikiDbSerializer
    {
        private const byte SlotDataSignal = 26;
        private const byte SeparatorSignal = 10;
        private const byte TextPointSeparatorSignal = 16;
        private const byte Slot1Signal = 8;
        private const byte Slot2Signal = 16;
        private const byte Slot3Signal = 24;
        private const byte NoSlotsSignal = 0;

        public static string Serialize(IEnumerable<Talisman> talismans)
        {
            var byteData = talismans.SelectMany(SerializeTalisman)
                                    .ToArray();

            return Convert.ToBase64String(byteData);
        }

        private static List<byte> SerializeTalisman(Talisman talisman)
        {
            var result = new List<byte>();

            if (talisman.Slot3 != SlotType.None)
            {
                result.Add((byte)talisman.Slot3);
                result.Add(Slot3Signal);
            }

            if (talisman.Slot2 != SlotType.None)
            {
                result.Add((byte)talisman.Slot2);
                result.Add(Slot2Signal);
            }

            if (talisman.Slot1 != SlotType.None)
            {
                result.Add((byte)talisman.Slot1);
                result.Add(Slot1Signal);
            }

            if (talisman.Slot1 == SlotType.None && talisman.Slot2 == SlotType.None && talisman.Slot3 == SlotType.None)
                result.Add(NoSlotsSignal);
            else
                result.Add((byte)result.Count);

            result.Add(SlotDataSignal);

            if (talisman.Skill2 != null)
                result.AddRange(SerializeSkill(talisman.Skill2));

            result.AddRange(SerializeSkill(talisman.Skill1));

            result.Add((byte)result.Count);
            result.Add(SeparatorSignal);

            result.Reverse();

            return result;
        }

        private static IEnumerable<byte> SerializeSkill(TalismanSkill skill)
        {
            var result = new List<byte>();

            result.Add((byte)skill.Points);
            result.Add(TextPointSeparatorSignal);
            result.AddRange(skill.Name.Select(c => (byte)c)
                                 .Reverse());
            result.Add((byte)skill.Name.Length);
            result.Add(SeparatorSignal);
            result.Add((byte)result.Count);
            result.Add(SeparatorSignal);

            return result;
        }

        public static IEnumerable<TalismanDto> Deserialize(string dataString)
        {
            var data = new Queue<byte>(Convert.FromBase64String(dataString));

            var talismans = SplitIntoTalismanData(data)
                            .Select(DeserializeTalismanData)
                            .ToList();
            return talismans;
        }

        private static TalismanDto DeserializeTalismanData(Queue<byte> data)
        {
            var result = new TalismanDto { Operation = TalismanOperation.Add };

            var (text1, points1) = DeserializeSkill(data);
            result.Skill1 = new TalismanSkill { Name = text1, Points = points1 };

            if (data.Peek() != SlotDataSignal)
            {
                var (text2, points2) = DeserializeSkill(data);
                result.Skill2 = new TalismanSkill { Name = text2, Points = points2 };
            }

            var (slot1, slot2, slot3) = DeserializeSlots(data);

            result.Slot1 = slot1;
            result.Slot2 = slot2;
            result.Slot3 = slot3;

            return result;
        }

        private static (SlotType slot1, SlotType slot2, SlotType slot3) DeserializeSlots(Queue<byte> data)
        {
            if (data.Dequeue() != SlotDataSignal)
                throw new Exception("error in data");

            var slotDataLength = data.Dequeue();

            var slots = Enumerable.Repeat(0, slotDataLength)
                                  .Select(_ => data.Dequeue())
                                  .Where(x => x is > 0 and <= 3)
                                  .Select(x => (SlotType)x)
                                  .Pad(3, SlotType.None)
                                  .ToList();

            return (slots[0], slots[1], slots[2]);
        }

        private static (string text, byte points) DeserializeSkill(Queue<byte> data)
        {
            if (data.Dequeue() != SeparatorSignal)
                throw new Exception("error in data");

            var skillLength = data.Dequeue();

            if (data.Dequeue() != SeparatorSignal)
                throw new Exception("error in data");

            var textLength = data.Dequeue();

            if (textLength != skillLength - 4)
                throw new Exception("error in data");

            var text = Encoding.ASCII.GetString(Enumerable.Range(0, textLength)
                                                          .Select(_ => data.Dequeue())
                                                          .ToArray());

            if (data.Dequeue() != TextPointSeparatorSignal)
                throw new Exception("error in data");

            var points = data.Dequeue();

            return (text, points);
        }

        private static IEnumerable<Queue<byte>> SplitIntoTalismanData(Queue<byte> data)
        {
            var result = new List<Queue<byte>>();
            while (data.Count > 0)
            {
                if (data.Dequeue() != SeparatorSignal)
                    throw new Exception("error in data");

                var dataLength = data.Dequeue();
                var talisman = new Queue<byte>(Enumerable.Range(0, dataLength)
                                                         .Select(_ => data.Dequeue())
                                                         .ToList());
                result.Add(talisman);
            }

            return result;
        }
    }
}
