using Hivemind.World.Entity.Moving;
using Hivemind.World.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.World.Colony
{
    public enum TaskType { BUILD, DESTROY, ATTACK, REPAIR }

    public class TaskManager
    {
        public List<BaseTask> Tasks = new List<BaseTask>();
        public TileMap Parent;

        public TaskManager(TileMap parent)
        {
            Parent = parent;
        }

        public void AddTask(BaseTask task)
        {
            Tasks.Add(task);
        }
    }

    public abstract class BaseTask
    {
        public TaskManager TaskManager;
        public TaskType Type;
        public float WorkRequired, WorkDone;
        public bool Complete = false;
        public int Priority;

        public BaseTask(int workRequired, TaskManager parent)
        {
            WorkRequired = workRequired;
            TaskManager = parent;
        }

        public virtual void DoWork(float work)
        {
            WorkDone += work;
            if (WorkDone > WorkRequired)
                TaskFinished();
        }

        public virtual void TaskFinished()
        {
            TaskManager.Tasks.Remove(this);
            Complete = true;
        }
    }

    public class BuildTask : BaseTask
    {
        public HoloTile Tile;

        public BuildTask(int workRequired, HoloTile tile, TileMap tileMap) : base(workRequired, tileMap.TaskManager)
        {
            Tile = tile;
        }

        public override void TaskFinished()
        {
            base.TaskFinished();
            Tile.Child.IsHolo = false;
            TaskManager.Parent.SetTile(Tile.Pos, Tile.Child);
            Tile t = TaskManager.Parent.GetTile(Tile.Pos);
            switch (Tile.Layer)
            {
                case Layer.WALL:
                    t.HoloWall.Destroy();
                    break;
                case Layer.FLOOR:
                    t.HoloFloor.Destroy();
                    break;
            }
        }
    }

    public class HaulTask : BaseTask
    {
        public Inventory Target;
        public readonly Dictionary<Material, float> TotalMaterials;
        public Dictionary<Material, float> MaterialsQueued = new Dictionary<Material, float>();
        public Dictionary<Material, float> MaterialsNeeded = new Dictionary<Material, float>();
        public Dictionary<Material, float> MaterialsStored => Target.GetMaterials();

        public HaulTask(TaskManager parent, Inventory target, Dictionary<Material, float> needed) : base(1, parent)
        {
            Target = target;
            TotalMaterials = needed;
            foreach(KeyValuePair<Material, float> m in TotalMaterials)
            {
                MaterialsQueued.Add(m.Key, 0);
                MaterialsNeeded.Add(m.Key, m.Value);
            }
        }

        public void QueueMaterial(Material m, float a)
        {
            MaterialsQueued[m] += a;
            MaterialsNeeded[m] -= a;
        }

        public void Deposit(Material m, float a)
        {
            MaterialsQueued[m] -= a;
            Target.Deposit(m, a);

            foreach (KeyValuePair<Material, float> mat in MaterialsNeeded)
            {
                if (mat.Value - MaterialsStored[mat.Key] > 0)
                {
                    return;
                }
            }

            TaskFinished();
        }

        public override void TaskFinished()
        {
            base.TaskFinished();
        }
    }
}
