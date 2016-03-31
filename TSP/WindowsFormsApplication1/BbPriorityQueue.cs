using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
    class BbPriorityQueue
    {
        TSPQueueWrapper current;

        List<TSPQueueWrapper>[] levelArray;

        int count;
        public BbPriorityQueue(int citiesSize)
        {
            levelArray = new List<TSPQueueWrapper>[citiesSize];
            for(int i=0; i<levelArray.Length; i++)
            {
                levelArray[i] = new List<TSPQueueWrapper>();
            }
        }

        public void Add(TSPState state)
        {
            count++;
            TSPQueueWrapper child = new TSPQueueWrapper(state);
            if (current != null)
            {
                current.children.Add(child);
            }
            levelArray[state.route.Count-1].Add(child);
        }

        public TSPState DeleteMin()
        {
            count--;

            if (current != null && current.children.Count > 0)
            {
                int minIndex = 0;
                for (int i = 1; i < current.children.Count; i++)
                {
                    if (current.children[i].state.BssfCost < current.children[minIndex].state.BssfCost)
                    {
                        minIndex = i;
                    }
                }
                TSPQueueWrapper child = current.children[minIndex];
                current.children.RemoveAt(minIndex);
                levelArray[child.state.route.Count-1].Remove(child);
                current = child;
            }
            else
            {
                List<TSPQueueWrapper> highestLevel = null;
                for (int i = 0; i < levelArray.Length; i++)
                {
                    if (levelArray[i].Count != 0)
                    {
                        highestLevel = levelArray[i];
                        break;
                    }
                }
                int minIndex = 0;
                for (int i = 1; i < highestLevel.Count; i++)
                {
                    if (highestLevel[i].state.BssfCost < highestLevel[i].state.BssfCost)
                    {
                        minIndex = i;
                    }
                }
                current = highestLevel[minIndex];
                highestLevel.Remove(current);
            }
            return current.state;
        }
            

        public bool Empty()
        {
            return count ==0;
        }

        public void pruneCurrentPath()
        {
            pruneBranch(current.children);
        }

        private void pruneBranch(List<TSPQueueWrapper> children)
        {
            for(int i=0; i<children.Count; i++)
            {
                levelArray[children[i].state.route.Count - 1].Remove(children[i]);
                pruneBranch(children[i].children);
            }
        }
    }

    class TSPQueueWrapper{
        public TSPState state {
            get;
        }
        public List<TSPQueueWrapper> children
        {
            get;
        }
        public TSPQueueWrapper(TSPState state)
        {
            this.state = state;
            this.children = new List<TSPQueueWrapper>();
        }
    }

       
}
