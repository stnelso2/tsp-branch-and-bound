using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
    //Modified queue that greedily goes down the tree, but if it
    //ever reaches the end of a path, then goes back up to the highest level
    //where anything is left. This balances going for low path cost, and 
    //finding complete paths. 
    class BbPriorityQueue
    {
        TSPQueueWrapper current;

        //keeps track of all nodes at each level of the tree.
        List<TSPQueueWrapper>[] levelArray;

        int count;

        public int Max {
            get; set;
        }
        

        public BbPriorityQueue(int citiesSize)
        {
            //initialize level array
            levelArray = new List<TSPQueueWrapper>[citiesSize];
            for(int i=0; i<levelArray.Length; i++)
            {
                levelArray[i] = new List<TSPQueueWrapper>();
            }
        }

        public void Add(TSPState state)
        {
            count++;
            //initialize the state in a wrapper that keeps track of it's children
            TSPQueueWrapper child = new TSPQueueWrapper(state);
            //current is the node that was recently popped off the queue
            if (current != null)
            {//add it as a child of the current one.
                current.children.Add(child);
            }
            //put the node in the proper level
            levelArray[state.route.Count-1].Add(child);
            if (count > Max)
                Max = count;
        }

        public TSPState DeleteMin()
        {
            count--;
            //if there are children of the currentNode, then keep going down the tree
            if (current != null && current.children.Count > 0)
            {//find the smallest path of all the nodes on that level
                int minIndex = 0;
                for (int i = 1; i < current.children.Count; i++)
                {
                    if (current.children[i].state.BssfCost < current.children[minIndex].state.BssfCost)
                    {
                        minIndex = i;
                    }
                }
                //set the current node, and remove it as a child and from it's level
                TSPQueueWrapper child = current.children[minIndex];
                current.children.RemoveAt(minIndex);
                levelArray[child.state.route.Count-1].Remove(child);
                current = child;
            }
            else
            {
             //if the current node was the last node of the chain, then go back to the highest level
             //with anything on it.
                List<TSPQueueWrapper> highestLevel = null;
                //find the highest level with anything on it. 
                for (int i = 0; i < levelArray.Length; i++)
                {
                    if (levelArray[i].Count != 0)
                    {
                        highestLevel = levelArray[i];
                        break;
                    }
                }
                //find the smallest path state on the highest level
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
           //recursive function that removes the current state and all its children
            for(int i=0; i<children.Count; i++)
            {
                levelArray[children[i].state.route.Count - 1].Remove(children[i]);
                count--;
                pruneBranch(children[i].children);
            }
        }
    }

    class TSPQueueWrapper{
        //wrapper class to allow state to know its children. 
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
