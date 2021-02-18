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
        public int WorkRequired, WorkDone;
        public int Priority;

        public BaseTask(int workRequired, TaskManager parent)
        {
            WorkRequired = workRequired;
            TaskManager = parent;
        }

        public virtual void DoWork(int work)
        {
            WorkDone += work;
            if (WorkDone > WorkRequired)
                TaskFinished();
        }

        public virtual void TaskFinished()
        {
            TaskManager.Tasks.Remove(this);
        }
    }

    public class BuildTask : BaseTask
    {
        HoloTile Tile;

        public BuildTask(int workRequired, HoloTile tile, TileMap tileMap) : base(workRequired, tileMap.Tasks)
        {
            Tile = tile;
        }

        public override void TaskFinished()
        {
            base.TaskFinished();
            TaskManager.Parent.SetTile(Tile.Pos, Tile.Child);
        }
    }
}
