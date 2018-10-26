using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthrock.Server.Score;
using Hearthrock.Server.Services;
using SabberStoneCore.Model;
using SabberStoneCore.Tasks;
using SabberStoneCore.Tasks.PlayerTasks;

namespace Hearthrock.Server.Simulation
{
    public class Node
    {
        public const int MaxSimulateDepth = 3;
        public const int MaxSimulateWidth = 10;

        public Node(Game originGame, int parentDepth, IScoringService scoringService, PlayerTask task)
        {
            Game = originGame.Clone();
            Depth = parentDepth + 1;
            this.scoringService = scoringService;
            Action = task;
            if (Action != null)
                Game.Process(Action);
            Score = scoringService.GetScore(Game.GetSceneData());
            CreateChildren();
        }

        public Node Parent { get; set; }
        public List<Node> Children { get; set; } = new List<Node>();
        public Game Game { get; set; }
        public PlayerTask Action { get; }
        private readonly IScoringService scoringService;
        public int Score { get; set; }
        public int Depth { get; }
        public Node BestResult => GetBestResult();

        public void CreateChildren()
        {
            if (Depth == MaxSimulateDepth || Action is EndTurnTask)
                return;
            foreach (var task in Game.CurrentPlayer.Options().Take(MaxSimulateWidth))
            {
                try
                {
                    var child = new Node(Game, Depth, scoringService, task);
                    child.Parent = this;
                    Children.Add(child);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        public Node GetBestResult()
        {
            var childrenOrdered = Children.OrderByDescending(c => c.BestResult.Score);
            var bestChild = childrenOrdered.FirstOrDefault();
            var bestNode = bestChild == null || bestChild.Score < this.Score ? this : bestChild;
            return bestNode;
        }
    }
}
