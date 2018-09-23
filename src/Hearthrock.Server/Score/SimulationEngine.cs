using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthrock.Contracts;
using SabberStoneCore.Actions;
using SabberStoneCore.Config;
using SabberStoneCore.Enums;
using SabberStoneCore.Model;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Model.Zones;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneCore.Tasks.SimpleTasks;
using SabberStoneCoreAi.Score;

namespace Hearthrock.Server.Score
{
    public class SimulationEngine
    {
        public SimulationEngine(RockScene scene)
        {
            currentScene = scene;
            game = BuildGameFromScene(currentScene);
        }

        private RockScene currentScene;
        private Dictionary<int, IEntity> idMap = new Dictionary<int, IEntity>();
        private Game game;

        public int SimulateAction(RockAction action)
        {
            //todo  add slot support.

            var score1 = CalculateGameScore(game);
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
                            var minionCard = Generic.DrawCard(p1, Cards.FromId(card.CardId));
                            game.Process(PlayCardTask.Any(p1, minionCard));
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
                            var spell = Generic.DrawCard(game.Player1, Cards.FromId(card.CardId));
                            game.Process(PlayCardTask.SpellTarget(game.Player1, spell, target));
                        }
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
                            var minion = Generic.DrawCard(game.Player1, Cards.FromId(card.CardId));
                            game.Process(PlayCardTask.MinionTarget(game.Player1, minion, target));
                        }
                        else
                        {
                            var spell = Generic.DrawCard(game.Player1, Cards.FromId(card.CardId));
                            game.Process(PlayCardTask.SpellTarget(game.Player1, spell, target));
                        }
                    }
                }
            }

            var score2 = CalculateGameScore(game);
            return score2 - score1;
        }

        private int CalculateGameScore(Game game)
        {
            var score = new PlayerScore();
            score.Controller = game.Player1;
            return score.Rate();
        }

        private Game BuildGameFromScene(RockScene scene)
        {
            var config = new GameConfig
            {
                StartPlayer = 1,
                Player1HeroClass = scene.Self.Hero.Class.ToSabberCardClass(),
                Player2HeroClass = scene.Opponent.Hero.Class.ToSabberCardClass(),
                FillDecks = true,
                FillDecksPredictably = true
            };
            var game = new Game(config, true);
            idMap.Add(scene.Self.Hero.RockId, game.Player1.Hero);
            idMap.Add(scene.Opponent.Hero.RockId, game.Player2.Hero);

            game.StartGame();
            //todo hearthrock does not pass armor\base-health to server 
            game.Player1.Hero.BaseHealth = 30;
            game.Player2.Hero.BaseHealth = 30;
            game.Player1.Hero.Damage = 30 - scene.Self.Hero.Health;
            game.Player2.Hero.Damage = 30 - scene.Opponent.Hero.Health;
            game.Player1.BaseMana = scene.Self.Resources;
            game.Player2.BaseMana = scene.Opponent.Resources;

            AddMinionsForPlayer(game.Player1.BoardZone, scene.Self.Minions);
            AddMinionsForPlayer(game.Player2.BoardZone, scene.Opponent.Minions);
            if (scene.Self.HasWeapon)
            {
                AddWeaponForPlayer(game.Player1.Hero, scene.Self);
            }

            if (scene.Opponent.HasWeapon)
            {
                AddWeaponForPlayer(game.Player2.Hero, scene.Opponent);
            }
            return game;
        }

        private void AddMinionsForPlayer(BoardZone boardZone, IEnumerable<RockMinion> minions)
        {
            foreach (var minion in minions)
            {
                var card = Cards.FromId(minion.CardId);
                var m = (Minion)Entity.FromCard(boardZone.Controller, card);
                m.Health = minion.Health;
                m.AttackDamage = minion.Damage;
                boardZone.Add(m);
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
