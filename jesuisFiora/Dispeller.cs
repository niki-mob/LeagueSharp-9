﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Extensions;

namespace jesuisFiora
{
    internal static class Dispeller
    {
        public static Menu Menu;

        static Dispeller()
        {
            new Dispel("Vladimir", "vladimirhemoplaguedebuff", SpellSlot.R).Add();
            new Dispel("Tristana", "tristanaechargesound", SpellSlot.E).Add();
            new Dispel("Karma", "karmaspiritbind", SpellSlot.W).Add();
            new Dispel("Karthus", "karthusfallenone", SpellSlot.R).Add();
            new Dispel("Leblanc", "leblancsoulshackle", SpellSlot.E).Add();
            new Dispel("Leblanc", "leblancsoulshacklem", SpellSlot.R).Add();
            new Dispel("Morgana", "soulshackles", SpellSlot.R).Add();
            new Dispel("Zed", "zedultexecute", SpellSlot.R).Add();
            new Dispel("Fizz", "fizzmarinerdoombomb", SpellSlot.R, 500).Add();
        }

        public static List<Dispel> Dispells => Dispel.GetDispelList();

        public static void Initialize(Menu menu)
        {
            foreach (var dispel in
                Dispel.GetDispelList().Where(d => HeroManager.Enemies.Any(h => h.ChampionName.Equals(d.ChampionName))))
            {
                menu.AddBool(
                    "Dispel" + dispel.ChampionName + dispel.BuffName,
                    "Dispel " + dispel.ChampionName + " " + dispel.Slot);
            }

            Menu = menu;
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            const int x = 100;
            var y = 100;
            foreach (var buffs in
                ObjectManager.Player.Buffs.Where(b => !b.SourceName.Equals(ObjectManager.Player.ChampionName)))
            {
                Drawing.DrawText(x, y, Color.Blue, buffs.Name + " " + buffs.SourceName);
                y += 20;
            }
            var w = SpellManager.W;

            if (!w.IsReady())
            {
                return;
            }

            foreach (var dispel in
                Dispells.Where(
                    d =>
                        (ObjectManager.Player.HasBuff(d.BuffName) &&
                         Menu.Item("Dispel" + d.ChampionName + d.BuffName) != null &&
                         Menu.Item("Dispel" + d.ChampionName + d.BuffName).IsActive())))
            {
                var buff = ObjectManager.Player.Buffs.FirstOrDefault(b => b.Name.ToLower().Equals(dispel.BuffName));
                if (buff == null || !buff.IsValid || !buff.IsActive)
                {
                    Console.WriteLine("CONTINUE");
                    continue;
                }

                var t = (buff.EndTime - Game.Time) * 1000f + 500 + dispel.Offset;
                var wT = w.Delay * 1000f + Game.Ping / 1.85f + 750;
                Console.WriteLine("T: {0} WT: {1}", t, wT);
                if (t < wT)
                {
                    var target = TargetSelector.GetTargetNoCollision(w);
                    Console.WriteLine("CAST DISPEL");
                    if (target != null && target.IsValidTarget(w.Range) && w.Cast(target).IsCasted())
                    {
                        return;
                    }

                    if (w.Cast(Game.CursorPos))
                    {
                        return;
                    }
                }
            }
        }
    }

    public class Dispel
    {
        private static readonly List<Dispel> DispelList = new List<Dispel>();
        public string BuffName;
        public string ChampionName;
        public int Offset;
        public SpellSlot Slot;

        public Dispel(string champName, string buff, SpellSlot slot, int offset = 0)
        {
            ChampionName = champName;
            BuffName = buff;
            Slot = slot;
            Offset = offset;
        }

        public void Add()
        {
            DispelList.Add(this);
        }

        public static List<Dispel> GetDispelList()
        {
            return DispelList;
        }
    }
}