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
using SabberStoneCoreAi.Score;

namespace Hearthrock.Server.Score
{
    public class SimulationEngine
    {
        public SimulationEngine(RockScene scene)
        {
            currentScene = scene;
        }

        private RockScene currentScene;
        private Dictionary<int, int> idMap = new Dictionary<int, int>();

        public int SimulateAction(RockAction action)
        {
            //var newScene = currentScene.DeepCopy();
            var game = BuildGameFromScene(currentScene);
            
            var score1 = CalculateGameScore(game);
            if (action.Objects.Count == 1)
            {
                // play out a card.
                var obj = currentScene.Self.GetObjectById(action.Objects[0]);
                var p1 = game.Player1;
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
                var target = currentScene.Self.GetObjectById(action.Objects[1]) ??
                             currentScene.Opponent.GetObjectById(action.Objects[1]);
                // if src is hero/minion ,do attack
            }

            var score2 = CalculateGameScore(game);
            return score2 - score1;
        }

        private int CalculateGameScore(Game game)
        {
            var score = new AggroScore();
            score.Controller = game.Player1;
            var p1score = score.Rate();
            score.Controller = game.Player2;
            var p2score = score.Rate();
            return p1score - p2score;
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
            game.StartGame();
            game.Player1.Hero.Health = scene.Self.Hero.Health;
            game.Player2.Hero.Health = scene.Opponent.Hero.Health;
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
                idMap.Add(minion.RockId, m.Id);
            }
        }

        private void AddWeaponForPlayer(Hero hero, RockPlayer player)
        {
            var card = Cards.FromId(player.Weapon.CardId);
            var weapon = new Weapon(hero.Controller, card, new ConcurrentDictionary<GameTag, int>());
            hero.AddWeapon(weapon);
        }
    }
}
