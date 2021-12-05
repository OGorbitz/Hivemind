using Hivemind.World;
using Hivemind.World.Entity.Tile;
using Hivemind.World.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.Utility
{
    public enum NodeType
    {
        WIRE,
        PRODUCER,
        CONSUMER,
        STORAGE,
        MIXED
    }



    public class PowerNetwork
    {
        public List<IPowerNode> Nodes = new List<IPowerNode>();
        public SpaseShip Core;
        public TileMap TileMap;

        public bool Dirty = true;

        private List<IPowerNode> _nodeCalculation = new List<IPowerNode>();
        private List<Point> _calculatedPoints = new List<Point>();


        static readonly int[,] Neighbors =
        {
                {0, -1},
                {1, 0},
                {0, 1},
                {-1, 0}
        };

        public PowerNetwork(TileMap tileMap)
        {
            TileMap = tileMap;
        }

        public void AddNode(IPowerNode node)
        {
            if (!Nodes.Contains(node))
            {
                Nodes.Add(node);
                node.OnNetworkJoin(this);
            }
        }

        public void RemoveNode(IPowerNode node)
        {
            if (Nodes.Contains(node))
            {
                Nodes.Remove(node);
                node.OnNetworkLeave();
            }
        }

        public void Update(GameTime gameTime)
        {
            if (Dirty)
                RecalculateNodes();
            
            float consumedPower = 0f;
            float producedPower = 0f;

            foreach (IPowerNode n in Nodes)
            {
                n.UpdatePower();

                switch (n.GetNodeType())
                {
                    case NodeType.PRODUCER:
                        producedPower += n.GetPower();
                        break;
                    case NodeType.CONSUMER:
                        consumedPower += n.GetPower();
                        break;
                }
            }

        }

        public void RecalculateNodes()
        {
            _nodeCalculation.Clear();
            _calculatedPoints.Clear();

            _nodeCalculation.Add(Core);

            for (int x = Core.Bounds.Left; x <= Core.Bounds.Right; x++)
            {
                for (int y = Core.Bounds.Top; y <= Core.Bounds.Bottom; y++)
                {
                    Point p = new Point(x, y);
                    CalculateNode(p);
                }
            }

            foreach(IPowerNode node in Nodes)
            {
                if (!_nodeCalculation.Contains(node))
                {
                    node.OnNetworkLeave();
                }
            }

            Nodes.Clear();
            Nodes.AddRange(_nodeCalculation);
            Dirty = false;
        }

        public void CalculateNode(Point position)
        {
            if (_calculatedPoints.Contains(position))
                return;
            _calculatedPoints.Add(position);

            Tile n = TileMap.GetTile(position);
            if(n != null && n.PowerCable != null)
            {
                if (!_nodeCalculation.Contains(n.PowerCable))
                {
                    _nodeCalculation.Add(n.PowerCable);

                    if(n.TileEntity != null && n.TileEntity is IPowerNode && !_nodeCalculation.Contains((IPowerNode)n.TileEntity))
                    {
                        _nodeCalculation.Add((IPowerNode)n.TileEntity);
                        for(int x = n.TileEntity.Bounds.Left; x < n.TileEntity.Bounds.Right; x++)
                        {
                            for(int y = n.TileEntity.Bounds.Top; y < n.TileEntity.Bounds.Bottom; y++)
                            {
                                Point p = new Point(x, y);
                                CalculateNode(p);
                            }
                        }
                    }
                }
            }

            for(int i = 0; i < 4; i++)
            {
                Point p = position + new Point(Neighbors[i, 0], Neighbors[i, 1]);

                Tile t = TileMap.GetTile(p);
                if (t != null && t.PowerCable != null)
                    CalculateNode(p);
            }
        }
    }

    public interface IPowerNode
    {
        public abstract NodeType GetNodeType();
        public abstract float GetPower();
        public abstract void UpdatePower();
        public abstract void OnNetworkJoin(PowerNetwork powerNetwork);
        public abstract void OnNetworkLeave();
    }
}
