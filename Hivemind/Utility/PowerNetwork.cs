using Hivemind.World;
using Hivemind.World.Entity.Tile;
using Hivemind.World.Tiles;
using Hivemind.World.Tiles.Utilities;
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
        public ITileMap TileMap;

        public bool Dirty = true;

        static readonly int[,] Neighbors =
        {
                {0, -1},
                {1, 0},
                {0, 1},
                {-1, 0}
        };

        /// <summary>
        /// Creates a new network, adds <see cref="node"/> to the network and checks for neighbors to the node.
        /// </summary>
        /// <param name="tileMap"></param>
        /// <param name="node"></param>
        public PowerNetwork(ITileMap tileMap)
        {
            TileMap = tileMap;
        }

        /// <summary>
        /// Should be called to add a node. Calls OnNetworkJoin() on the node.
        /// </summary>
        /// <param name="node"></param>
        public void AddNode(IPowerNode node)
        {
            if (Nodes.Contains(node))
                return;
            
            Nodes.Add(node);
            node.OnNetworkJoin(this);
            
        }
        
        /// <summary>
        /// Should be called to remove a node. Calls OnNetworkLeave() on the node.
        /// </summary>
        /// <param name="node"></param>
        public void RemoveNode(IPowerNode node)
        {
            if (!Nodes.Contains(node))
                return;
            
            Nodes.Remove(node);
            node.OnNetworkLeave();
        }

        /// <summary>
        /// Updates power calculations for network
        /// </summary>
        /// <param name="gameTime"></param>
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


        /// <summary>
        /// Network has been changed, recalculate attached nodes.
        /// </summary>
        public void RecalculateNodes()
        {
            List<IPowerNode> connectedNodes = new List<IPowerNode>();

            CalculateNodes(Core, connectedNodes);

            //Check if each node in the powernetwork is in the newly calculated list of connected nodes. If not, remove it.
            foreach(var node in Nodes)
            {
                if (!connectedNodes.Contains(node))
                    RemoveNode(node);
            }

            //Add each node in the newly calculated list to the network. It will fail the if check in AddNode() if it is already in.
            foreach(var node in connectedNodes)
            {
                AddNode(node);
            }
        }

        /// <summary>
        /// Recursively check each nodes' neighbors and non duplicatively add them to list nodes
        /// </summary>
        /// <param name="node">The node to check</param>
        /// <param name="nodes">The list to add connected nodes to</param>
        public void CalculateNodes(IPowerNode node, List<IPowerNode> nodes)
        {
            foreach(var n in node.GetConnections())
            {
                if (!nodes.Contains(n))
                {
                    nodes.Add(n);
                    CalculateNodes(n, nodes);
                }
            }
        }

        /* Wrote code for merging and splitting which ended up not being used. May reuse someday. ¯\_(ツ)_/¯
         * 
         * 
        /// <summary>
        /// Moves all nodes in the given network into this network
        /// </summary>
        /// <param name="network"></param>
        public void Merge(PowerNetwork network)
        {
            //For optimization, it merges the nodes into the larger network. 

            //If this network has more nodes, merge the parameter network into this one
            if(network.Nodes.Count < this.Nodes.Count)
            {
                foreach (var n in network.Nodes)
                {
                    n.OnNetworkLeave();
                    n.OnNetworkJoin(this);
                }
            }
            //Otherwise, merge into parameter network
            else
            {
                foreach (var n in this.Nodes)
                {
                    n.OnNetworkLeave();
                    n.OnNetworkJoin(network);
                }
            }
        }

        /// <summary>
        /// Splits the network by removing node
        /// </summary>
        /// <param name="node"></param>
        public void Split(IPowerNode node)
        {
            var neighbors = node.GetConnections();

            var scanIndex = new Dictionary<IPowerNode, int>();
            
            //dimension refers to starting node for the scan.
            var scanListOpen = new List<IPowerNode>[neighbors.Count];
            var scanListClosed = new List<IPowerNode>[neighbors.Count];
            var scanListFinished = new bool[neighbors.Count];

            for (int i = 0; i < neighbors.Count; i++)
            {
                scanListOpen[i] = new List<IPowerNode>();
                scanListClosed[i] = new List<IPowerNode>();
                scanListFinished[i] = false;
            }

            var continueLoop = true;
            while (continueLoop)
            {
                continueLoop = false;
                
                for(int i = 0; i < neighbors.Count; i++)
                {
                    if (scanListFinished[i])
                        continue;
                    
                    //If there are no more values in the open list, this scan list is complete
                    if(scanListOpen[i].Count == 0)
                    {
                        scanListFinished[i] = true;
                        continue;
                    }

                    continueLoop = true;
                    
                    //Get first value in open list
                    var scanned = scanListOpen[i][0];
                    //Remove from open list
                    scanListOpen[i].RemoveAt(0);
                    //Add to closed list
                    scanListClosed[i].Add(scanned);
                    
                    //Get neighbors
                    
                    foreach(var neighbor in scanned.GetConnections())
                    {
                        //This node has already been scanned. Merge scans
                        if (scanIndex.ContainsKey(neighbor))
                        {
                            scanListFinished[i] = true;
                            
                            int k = scanIndex.GetValueOrDefault(neighbor);
                            scanListOpen[i].AddRange(scanListOpen[k]);
                            scanListClosed[i].AddRange(scanListClosed[k]);
                        }
                        //Node hasn't been scanned yet, add to open list
                        else
                        {
                            scanListOpen[i].Add(neighbor);
                        }
                    }
                }
            }

            //Create new networks for each scan list
            foreach (var list in scanListClosed)
            {
                if (list.Count > 0)
                    new PowerNetwork(TileMap, list);
            }

        }*/

        public void Destroy()
        {
            GC.SuppressFinalize(this);
        }
    }



    public interface IPowerNode
    {
        public abstract NodeType GetNodeType();
        public abstract List<IPowerNode> GetConnections();
        public abstract PowerNetwork GetPowerNetwork();
        public abstract float GetPower();
        public abstract void UpdatePower();
        public abstract void OnNetworkJoin(PowerNetwork powerNetwork);
        public abstract void OnNetworkLeave();
    }
}
