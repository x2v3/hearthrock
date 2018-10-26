using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthrock.Contracts;
using Hearthrock.Server.Services;
using Hearthrock.Server.Simulation;
using SabberStoneCore.Actions;
using SabberStoneCore.Config;
using SabberStoneCore.Enums;
using SabberStoneCore.Model;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Model.Zones;
using SabberStoneCore.Tasks;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneCore.Tasks.SimpleTasks;
using SabberStoneCoreAi.Nodes;
using SabberStoneCoreAi.Score;

namespace Hearthrock.Server.Score
{
    public class SimulationEngine
    {
        public SimulationEngine(RockScene scene, ActionLogService dbLogService ,IScoringService scoringService)
        {
            currentScene = scene;
            game = BuildGameFromScene(currentScene);
            this.dbLogService = dbLogService;
            this.scoringService = scoringService;
        }


        private RockScene currentScene;
        private Dictionary<int, IEntity> idMap = new Dictionary<int, IEntity>();
        private Game game;
        private int maxSimulationDepth = 10;
        private ActionLogService dbLogService;
        private IScoringService scoringService;

        public int SimulateAction(RockAction action, bool simulateFollowingOptions = true)
        {
            var finalScore = 0;
            try
            {
                
                var p1 = game.Player1;
                if (action.Objects.Count == 1)
                {
                    // play out a card.
                    var obj = currentScene.Self.GetObjectById(action.Objects[0]);
                    if (obj != null)
                    {
                        if (obj is RockCard card)
                        {
                            if (card.CardId == game.Player1.Hero.HeroPower.Card.Id)
                            {
                                game.Process(HeroPowerTask.Any(p1));
                            }
                            else
                            {
                                var handMinion = p1.HandZone.FirstOrDefault(c => c.Card.Id == card.CardId);
                                var minion = handMinion ?? Generic.DrawCard(game.Player1, Cards.FromId(card.CardId));
                                game.Process(PlayCardTask.Any(p1, minion, null, action.Slot));
                            }
                        }
                    }
                }
                else if (action.Objects.Count == 2)
                {
                    var src = currentScene.Self.GetObjectById(action.Objects[0]);
                    var targetRockId = action.Objects[1];
                    var target = idMap[targetRockId];
                    // if src is hero/minion ,do attack
                    if (idMap.Keys.Contains(action.Objects[0]))
                    {
                        if (src is RockCard card)
                        {
                            if (card.CardType == RockCardType.Minion)
                            {
                                var gamesrc = idMap[card.RockId];
                                game.Process(MinionAttackTask.Any(game.Player1, gamesrc, target));
                            }
                            else
                            {
                                var handSpell = p1.HandZone.FirstOrDefault(c => c.Card.Id == card.CardId);
                                var spell = handSpell ?? Generic.DrawCard(game.Player1, Cards.FromId(card.CardId));
                                game.Process(PlayCardTask.SpellTarget(game.Player1, spell, target));
                            }
                        }
                        else if (src is RockMinion minion)
                        {
                            var gamesrc = idMap[minion.RockId];
                            game.Process(MinionAttackTask.Any(game.Player1, gamesrc, target));

                        }
                    }
                    else
                    {
                        if (src is RockCard card)
                        {
                            if (card.CardId == game.Player1.Hero.HeroPower.Card.Id)
                            {
                                game.Process(HeroPowerTask.Any(p1, idMap[targetRockId]));
                            }
                            else if (card.CardType == RockCardType.Minion)
                            {
                                var handMinion = p1.HandZone.FirstOrDefault(c => c.Card.Id == card.CardId);
                                var minion = handMinion ?? Generic.DrawCard(game.Player1, Cards.FromId(card.CardId));
                                game.Process(PlayCardTask.Any(game.Player1, minion, target, action.Slot));
                            }
                            else
                            {
                                var handSpell = p1.HandZone.FirstOrDefault(c => c.Card.Id == card.CardId);
                                var spell = handSpell ?? Generic.DrawCard(game.Player1, Cards.FromId(card.CardId));
                                game.Process(PlayCardTask.SpellTarget(game.Player1, spell, target));

                            }
                        }
                    }
                }

                if (simulateFollowingOptions)
                {
                    var bestResult = FindBestFollowingResult(game);
                    game.Process(EndTurnTask.Any(game.Player1));
                    finalScore = CalculateGameScore(bestResult);
                }
                else
                {
                    game.Process(EndTurnTask.Any(game.Player1));
                    finalScore = CalculateGameScore(game);
                }

            }
            catch (Exception e)
            {
                dbLogService?.AddErrorLogAsnycNoResult(currentScene, e);
            }
            return finalScore;
        }

        private Game FindBestFollowingResult(Game g)
        {
            try
            {
                var node = new Node(g,0,scoringService,null);
                return node.BestResult.Game;
            }
            catch (Exception e)
            {
                dbLogService?.AddErrorLogAsnycNoResult(currentScene, e);
            }

            return g;
        }

        private int CalculateGameScore(Game game)
        {
            //var score = new PlayerScore { Controller = game.Player1 };
            //return score.Rate();
            var data = game.GetSceneData();
            // turn and hero class is store and trained with RockScene, so we have to use RockXXX
            data.Round = currentScene.Turn;
            data.SelfHeroClass = (int) currentScene.Self.Hero.Class;
            data.OpHeroClass = (int) currentScene.Opponent.Hero.Class;
            var score = 0;
            try
            {
                score = scoringService.GetScore(data);
            }
            catch (Exception e)
            {
                // fallback
                
                var ps  = new PlayerScore { Controller = game.Player1 };
                return ps.Rate();
            }

            return score;
        }

        private Game BuildGameFromScene(RockScene scene)
        {
            var config = new GameConfig
            {
                StartPlayer = 1,
                Player1HeroClass = scene.Self.Hero.Class.ToSabberCardClass(),
                Player2HeroClass = scene.Opponent.Hero.Class.ToSabberCardClass(),
                FillDecks = true,
                FillDecksPredictably = true,
                SkipMulligan = true,
            };
            var game = new Game(config, true);
            idMap.Add(scene.Self.Hero.RockId, game.Player1.Hero);
            idMap.Add(scene.Opponent.Hero.RockId, game.Player2.Hero);

            //todo hearthrock does not pass armor\base-health to server 
            game.Player1.Hero.BaseHealth = 30;
            game.Player2.Hero.BaseHealth = 30;
            game.Player1.Hero.Damage = 30 - scene.Self.Hero.Health;
            game.Player2.Hero.Damage = 30 - scene.Opponent.Hero.Health;
            game.Player1.BaseMana = scene.Self.Resources;
            game.Player2.BaseMana = scene.Opponent.Resources;

            AddMinionsForPlayer(game.Player1.BoardZone, scene.Self.Minions);
            AddMinionsForPlayer(game.Player2.BoardZone, scene.Opponent.Minions);

            AddHandCardsForPlayer(game.Player1, scene.Self.Cards);

            if (scene.Self.HasWeapon)
            {
                AddWeaponForPlayer(game.Player1.Hero, scene.Self);
            }

            if (scene.Opponent.HasWeapon)
            {
                AddWeaponForPlayer(game.Player2.Hero, scene.Opponent);
            }

            game.StartGame();
            game.MainReady();
            return game;
        }

        private void AddMinionsForPlayer(BoardZone boardZone, IEnumerable<RockMinion> minions)
        {
            foreach (var minion in minions)
            {
                var card = Cards.FromId(minion.CardId);
                var m = (Minion)Entity.FromCard(boardZone.Controller, card);
                boardZone.Add(m);
                m.Reset();
                m.BaseHealth = minion.BaseHealth;
                m.Health = minion.Health;
                // hearth rock does not pass damage data to server.
                var possibleDamage = minion.BaseHealth - minion.Health;
                m.Damage = possibleDamage >= 0 ? possibleDamage : 0;
                m.AttackDamage = minion.Damage;
                idMap.Add(minion.RockId, m);
            }
        }

        private void AddWeaponForPlayer(Hero hero, RockPlayer player)
        {
            var card = Cards.FromId(player.Weapon.CardId);
            var weapon = (Weapon)Entity.FromCard(hero.Controller, card);
            weapon.AttackDamage = player.Weapon.Damage;
            weapon.Durability = player.Weapon.Health;
            hero.AddWeapon(weapon);
            idMap.Add(player.Weapon.RockId, hero.Weapon);
        }

        private void AddHandCardsForPlayer(Controller player, List<RockCard> cards)
        {
            while (player.HandZone.Count > 0)
            {
                player.HandZone.Remove(player.HandZone[0]);
            }
            foreach (var card in cards)
            {
                Generic.DrawCard(player, Cards.FromId(card.CardId));
            }
        }


        /// <summary>
        /// set used mana to 0 for both players.
        /// </summary>
        public void ResetHeroMana()
        {
            game.Player1.UsedMana = 0;
            game.Player2.UsedMana = 0;
        }
    }
}
