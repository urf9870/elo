using System;
using System.Linq;

using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace Gangnam_Dr.Mundo
{
    class Program
    {
        private static Menu Menu, CMenu, HMenu, LCMenu, JCMenu, MMenu, DMenu;
        private static Spell.Skillshot _q;
        private static Spell.Active _w;
        private static Spell.Active _e;
        private static Spell.Active _r;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "DrMundo")
            {
                return;
            }

            _q = new Spell.Skillshot(SpellSlot.Q, 1000, EloBuddy.SDK.Enumerations.SkillShotType.Linear, 250, 1850, 60) { MinimumHitChance = EloBuddy.SDK.Enumerations.HitChance.High };
            _q.AllowedCollisionCount = 0;

            _w = new Spell.Active(SpellSlot.W, 325);

            _e = new Spell.Active(SpellSlot.E, 150);

            _r = new Spell.Active(SpellSlot.R);

            Menu = MainMenu.AddMenu("Dr.Mundo", "Ten Percent Dr.Mundo".ToLower(),"Ten Percent Dr.Mundo");
            Menu.AddLabel("Gangnam");
            Menu.AddLabel("Ten Percent Dr.Mundo Ver 0.0.0.91 (Beta)");
            Menu.AddLabel("Last Update 2016.03.20");
            Menu.AddLabel("Plz give me Many FeedBack :)");
            Menu.AddLabel("Change Log");
            Menu.AddLabel("0.0.0.9  - Release");
            Menu.AddLabel("0.0.0.91 - Auto Harass Added");

            CMenu = Menu.AddSubMenu("Combo", "CMenu");
            CMenu.Add("CQ", new CheckBox("Use Q"));
            CMenu.Add("CW", new CheckBox("Use W"));
            CMenu.Add("CE", new CheckBox("Use E"));
            CMenu.Add("CR", new CheckBox("Use R", false));
            CMenu.Add("CH", new Slider("R If HP", 30, 0, 100));
            CMenu.AddSeparator();

            HMenu = Menu.AddSubMenu("Harass", "HMenu");
            HMenu.Add("HQ", new CheckBox("Use Q"));
            HMenu.Add("HW", new CheckBox("Use W"));
            HMenu.Add("HE", new CheckBox("Use E"));
            HMenu.Add("HA", new CheckBox("Auto Harass Q", false));
            HMenu.AddSeparator();

            LCMenu = Menu.AddSubMenu("LaneClear", "LCMenu");
            LCMenu.Add("LCQ", new CheckBox("Use Q"));
            LCMenu.Add("LCW", new CheckBox("Use W", false));
            LCMenu.Add("LCE", new CheckBox("Use E"));
            LCMenu.AddSeparator();

            JCMenu = Menu.AddSubMenu("JungleClear", "JCMenu");
            JCMenu.Add("JCQ", new CheckBox("Use Q"));
            JCMenu.Add("JCW", new CheckBox("Use W", false));
            JCMenu.Add("JCE", new CheckBox("Use E"));
            JCMenu.AddSeparator();

            MMenu = Menu.AddSubMenu("Misc", "MMenu");
            MMenu.Add("MO", new Slider("Auto W Off If HP", 10, 0, 100));
            MMenu.Add("MA", new CheckBox("Smart W Logic"));
            MMenu.AddSeparator();

            DMenu = Menu.AddSubMenu("Drawing", "DMenu");
            DMenu.Add("DO", new CheckBox("Disable Drawings", false));
            DMenu.Add("DQ", new CheckBox("Draw Q Range"));
            DMenu.Add("DW", new CheckBox("Draw W Range"));
            DMenu.Add("DH", new CheckBox("Draw HP"));
            DMenu.AddSeparator();

            Menu.AddSeparator();

            Game.OnUpdate += Game_OnUpdate;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
            Drawing.OnDraw += Drawing_OnDraw;
        }
        
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DMenu["DO"].Cast<CheckBox>().CurrentValue)
            {
                return;
            }

            if (DMenu["DQ"].Cast<CheckBox>().CurrentValue)
            {
                if (_q.IsReady())
                {
                    Circle.Draw(Color.Green, _q.Range, ObjectManager.Player.Position);
                }
            }

            if (DMenu["DW"].Cast<CheckBox>().CurrentValue)
            {
                if (_w.IsReady())
                {
                    Circle.Draw(Color.Green, _w.Range, ObjectManager.Player.Position);
                }
            }

            if (DMenu["DH"].Cast<CheckBox>().CurrentValue)
            {
                var pos = Drawing.WorldToScreen(Player.Instance.Position);
                Drawing.DrawText(pos.X, pos.Y + 40, System.Drawing.Color.White, "HP : " + ObjectManager.Player.HealthPercent);
            }
        }

        private static void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (target is AIHeroClient)
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    if (CMenu["CE"].Cast<CheckBox>().CurrentValue)
                    {
                        if (_e.IsReady())
                        {
                            _e.Cast();
                        }
                    }
                }
                else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                {
                    if (HMenu["HE"].Cast<CheckBox>().CurrentValue)
                    {
                        if (_e.IsReady())
                        {
                            _e.Cast();
                        }
                    }
                }
            }
            else
            {
                if (target is Obj_AI_Minion)
                {
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                    {
                        if (LCMenu["LCE"].Cast<CheckBox>().CurrentValue)
                        {
                            if (_e.IsReady())
                            {
                                var t = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, 
                                    ObjectManager.Player.Position, 150f).FirstOrDefault(x => x.IsValidTarget(150f));
                                if (t != null)
                                {
                                    _e.Cast();
                                }
                            }
                        }
                    }
                }

                if (JCMenu["JCE"].Cast<CheckBox>().CurrentValue)
                {
                    if (_e.IsReady())
                    {
                        var t = EntityManager.MinionsAndMonsters.GetJungleMonsters(ObjectManager.Player.Position, _q.Range).OrderBy(x => x.MaxHealth)
                            .FirstOrDefault(x => x.IsValidTarget());

                        if (t != null)
                        {
                            _e.Cast();
                        }
                    }
                }
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalker.CanMove)
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    if (CMenu["CQ"].Cast<CheckBox>().CurrentValue)
                    {
                        if (_q.IsReady())
                        {
                            var target = TargetSelector.GetTarget(_q.Range, DamageType.Magical);
                            if (target.IsValidTarget(_q.Range))
                            {
                                _q.Cast(target);
                            }
                        }
                    }

                    if (ObjectManager.Player.IsHealthPercentOkey(MMenu["MO"].Cast<Slider>().CurrentValue))
                    {
                        if (CMenu["CW"].Cast<CheckBox>().CurrentValue)
                        {
                            var target = TargetSelector.GetTarget(_w.Range + 20f, DamageType.Magical);

                            if (target != null)
                            {
                                if (_w.Handle.ToggleState == 1)
                                {
                                    _w.Cast();
                                }
                            }
                        }
                    }
                }

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                {
                    if (HMenu["HQ"].Cast<CheckBox>().CurrentValue)
                    {
                        if (_q.IsReady())
                        {
                            var target = TargetSelector.GetTarget(_q.Range, DamageType.Magical);
                            if (target.IsValidTarget(_q.Range))
                            {
                                _q.Cast(target);
                            }
                        }
                    }

                    if (ObjectManager.Player.IsHealthPercentOkey(MMenu["MO"].Cast<Slider>().CurrentValue))
                    {
                        if (HMenu["HW"].Cast<CheckBox>().CurrentValue)
                        {
                            if (_w.IsReady())
                            {
                                var target = TargetSelector.GetTarget(_w.Range + 20f, DamageType.Magical);
                                if (target != null)
                                {
                                    if (_w.Handle.ToggleState == 1)
                                    {
                                        _w.Cast();
                                    }
                                }
                            }
                        }
                    }
                }

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                {
                    if (LCMenu["LCQ"].Cast<CheckBox>().CurrentValue)
                    {
                        if (_q.IsReady())
                        {
                            var target = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, ObjectManager.Player.Position, _q.Range)
                                .OrderBy(x => x.Health).FirstOrDefault(x => x.IsValidTarget(_q.Range));

                            if (target != null)
                            {
                                if (target.BaseSkinName != "ward")
                                {
                                    _q.Cast(target);
                                }
                            }
                        }
                    }

                    if (ObjectManager.Player.IsHealthPercentOkey(MMenu["MO"].Cast<Slider>().CurrentValue))
                    {
                        if (LCMenu["LCW"].Cast<CheckBox>().CurrentValue)
                        {
                            if (_w.IsReady())
                            {
                                var target = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                                    ObjectManager.Player.Position, _w.Range + 20f).ToList();

                                if (target.Count >= 1)
                                {
                                    if (_w.Handle.ToggleState == 1)
                                    {
                                        _w.Cast();
                                    }
                                }
                            }
                        }
                    }
                }

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                {
                    if (JCMenu["JCQ"].Cast<CheckBox>().CurrentValue)
                    {
                        if (_q.IsReady())
                        {
                            var target = EntityManager.MinionsAndMonsters.GetJungleMonsters(ObjectManager.Player.Position, _q.Range)
                                .OrderBy(x => x.MaxHealth).FirstOrDefault(x => x.IsValidTarget(_q.Range));

                            if (target != null)
                            {
                                _q.Cast(target);
                            }
                        }
                    }

                    if (ObjectManager.Player.IsHealthPercentOkey(MMenu["MO"].Cast<Slider>().CurrentValue))
                    {
                        if (JCMenu["JCW"].Cast<CheckBox>().CurrentValue)
                        {
                            if (_w.IsReady())
                            {
                                var target = EntityManager.MinionsAndMonsters.GetJungleMonsters(ObjectManager.Player.Position, _q.Range)
                                    .OrderBy(x => x.MaxHealth).FirstOrDefault(x => x.IsValidTarget(_w.Range + 20f));

                                if (target != null)
                                {
                                    if (_w.Handle.ToggleState == 1)
                                    {
                                        _w.Cast();
                                    }
                                }
                            }
                        }
                    }
                }

                if (MMenu["MA"].Cast<CheckBox>().CurrentValue)
                {
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                    {
                        var miniontarget = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                            ObjectManager.Player.Position, _w.Range + 100f).ToList();
                        var jungletarget = EntityManager.MinionsAndMonsters.GetJungleMonsters(ObjectManager.Player.Position, _w.Range + 200f).OrderBy(x => x.MaxHealth).ToList();

                        if (miniontarget.Count < 1 && jungletarget.Count < 1)
                        {
                            if (_w.Handle.ToggleState != 1)
                            {
                                _w.Cast();
                            }
                        }
                    }
                    else
                    {
                        if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                        {
                            var target = EntityManager.MinionsAndMonsters.GetJungleMonsters(ObjectManager.Player.Position, _w.Range + 200f).OrderBy(x => x.MaxHealth).ToList();

                            if (target.Count < 1)
                            {
                                if (_w.Handle.ToggleState != 1)
                                {
                                    _w.Cast();
                                }
                            }
                        }
                        else
                        {
                            var target = TargetSelector.GetTarget(float.MaxValue, DamageType.Magical);
                            if (target != null)
                            {
                                if (target.Position.Distance(ObjectManager.Player.Position) >= 599.5f)
                                {
                                    if (_w.Handle.ToggleState != 1)
                                    {
                                        _w.Cast();
                                    }
                                }
                            }
                            else
                            {
                                if (_w.Handle.ToggleState != 1)
                                {
                                    _w.Cast();
                                }
                            }
                        }
                    }
                }
            }
            if (!(ObjectManager.Player.IsHealthPercentOkey(CMenu["CH"].Cast<Slider>().CurrentValue)))
            {
                if (!ObjectManager.Player.IsRecalling())
                {
                    if (CMenu["CR"].Cast<CheckBox>().CurrentValue)
                    {
                        if (_r.IsReady())
                        {
                            _r.Cast();
                        }
                    }
                }
            }

            if (HMenu["HA"].Cast<CheckBox>().CurrentValue)
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    return;
                }

                if (_q.IsReady())
                {
                    var target = TargetSelector.GetTarget(_q.Range, DamageType.Magical);
                    if (target.IsValidTarget(_q.Range))
                    {
                        _q.Cast(target);
                    }
                }
            }
        }      
    }
}
