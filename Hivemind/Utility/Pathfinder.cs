using Hivemind.World;
using Hivemind.World.Tile;
using Hivemind.World.Tile.Floor;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.Utility
{
    public enum NodeState { OPEN, CLOSED, PATH, BLOCKED}

    public delegate PathNode Calc(Vector2 position, Vector2 start, float startDistance);

    public class Pathfinder
    {
        public static readonly Vector2 UP = new Vector2(0, -1);
        public static readonly Vector2 DOWN = new Vector2(0, 1);
        public static readonly Vector2 LEFT = new Vector2(-1, 0);
        public static readonly Vector2 RIGHT = new Vector2(1, 0);
        public static readonly Vector2 UPLEFT = new Vector2(-1, -1);
        public static readonly Vector2 UPRIGHT = new Vector2(1, -1);
        public static readonly Vector2 DOWNRIGHT = new Vector2(1, 1);
        public static readonly Vector2 DOWNLEFT = new Vector2(-1, 1);
        public static readonly Vector2[] Neighbor = { UP, DOWN, LEFT, RIGHT , UPLEFT, UPRIGHT, DOWNRIGHT, DOWNLEFT };

        Vector2 Start, End;
        public Dictionary<Vector2, PathNode> Nodes;
        public List<PathNode> Path;
        public bool Finished = false;
        public bool Solution = false;
        public int MaxCycles = 0, CurrentCycle = 0;

        public Pathfinder(Vector2 start, Vector2 end, int maxCycles)
        {
            Start = start;
            End = end;
            MaxCycles = maxCycles;

            Nodes = new Dictionary<Vector2, PathNode>();
            Nodes.Add(Start, CalcNode(Start, Start, 0));
        }

        public Pathfinder(Vector2 start, Vector2 end, int maxCycles, Calc calcNode) : this(start, end, maxCycles)
        {
            CalcNode = calcNode;
        }

        public Calc CalcNode = (Vector2 position, Vector2 start, float startDistance) =>
        {
            PathNode node = new PathNode(position, start, startDistance);
            if(position.X >= 0 && position.X < WorldManager.GetActiveTileMap().Size && position.Y >= 0 && position.Y < WorldManager.GetActiveTileMap().Size)
            {
                node.Blocked = false;

                BaseTile wall = WorldManager.GetActiveTileMap().GetTile(position, Layer.WALL);
                BaseFloor floor = WorldManager.GetActiveTileMap().GetFloor(position);

                if (wall == null)
                {
                    node.Blocked = false;
                    node.State = NodeState.OPEN;
                }
                else
                {
                    if (wall.Resistance < 0 || floor == null)
                    {
                        node.Blocked = true;
                        node.State = NodeState.BLOCKED;
                    }
                    else
                    {
                        node.Blocked = false;
                        node.Resistance = floor.Resistance + wall.Resistance;
                        node.State = NodeState.OPEN;
                    }
                }
            }
            else
            {
                node.Blocked = true;
                node.State = NodeState.BLOCKED;
            }

            return node;
        };


        public void Cycle()
        {
            if (!Finished)
            {
                PathNode best = null;
                float bestheuristic = 0.0f;

                //Check for best open node in list
                foreach (KeyValuePair<Vector2, PathNode> p in Nodes)
                {
                    PathNode n = p.Value;
                    if(n.State == NodeState.OPEN)
                    {
                        if(best == null)
                        {
                            best = n;
                            bestheuristic = best.Heuristic(Start, End);
                            continue;
                        }

                        float h = n.Heuristic(Start, End);
                        if(h < bestheuristic)
                        {
                            best = n;
                            bestheuristic = best.Heuristic(Start, End);
                            continue;
                        }
                    }
                }

                //Add neighbors, close node
                if(best != null)
                {
                    best.State = NodeState.CLOSED;
                    
                    for(int i = 0; i < Neighbor.Length; i++)
                    {
                        Vector2 p = best.Pos + Neighbor[i];
                        Nodes.TryAdd(p, CalcNode(p, Start, best.StartDistance + best.Distance(p)));

                        //node is end node. Solve.
                        if (p == End)
                        {
                            Path = new List<PathNode>();

                            PathNode currentNode = Nodes[End];
                            Path.Add(currentNode);

                            while (true)
                            {
                                PathNode smallestNode = null;

                                //Find smallest node next to current node
                                for (int j = 0; j < Neighbor.Length; j++)
                                {
                                    Vector2 testnode = currentNode.Pos + Neighbor[j];
                                    if (Nodes.ContainsKey(testnode))
                                    {
                                        PathNode n = Nodes[testnode];

                                        if (n.State == NodeState.CLOSED && n.StartDistance < currentNode.StartDistance)
                                        {
                                            smallestNode = n;
                                        }
                                    }
                                }

                                if (smallestNode == currentNode || smallestNode == null)
                                {
                                    Finished = true;
                                    break;
                                }

                                //Add smallestnode to list of path
                                Path.Add(smallestNode);
                                currentNode = smallestNode;

                                //Start is found
                                if(smallestNode.Pos == Start)
                                {
                                    Solution = true;
                                    Finished = true;
                                    break;
                                }
                            }

                            if(Nodes[End].State != NodeState.OPEN)
                            {
                                Solution = false;
                            }
                        }
                    }
                    CurrentCycle += 1;
                }
                //If no new nodes are available
                else
                {
                    Finished = true;
                }
                //If max number of cycles is exceeded
                if(CurrentCycle > MaxCycles)
                {
                    Finished = true;
                }
            }
            if (Finished)
                Nodes = null;
        }
    }

    public class PathNode
    {
        public Vector2 Pos;
        public float Resistance = 0, StartDistance = 0, IdealStartDistance;
        public bool Blocked;
        public NodeState State;

        public PathNode(Vector2 pos, Vector2 start, float startDistance)
        {
            Pos = pos;
            StartDistance = startDistance;
            IdealStartDistance = Distance(start);
        }

        public float Manhattan(Vector2 v)
        {
            return Math.Abs(v.X - Pos.X) + Math.Abs(v.Y - Pos.Y);
        }

        public float Distance(Vector2 v)
        {
            return Math.Abs((v - Pos).Length());
        }

        public float Heuristic(Vector2 start, Vector2 end)
        {
            return 1.5f * (StartDistance - Distance(start)) + Distance(end);
        }
    }
}
