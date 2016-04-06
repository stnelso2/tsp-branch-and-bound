using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace TSP
{
    class BbSolver
    {
        ReducedCostMatrix rcm;
        TSPState bssf = null;

        public BbSolver(City[] cities, ArrayList route, double bssfCost)
        {
            //this is my best solution so far, which is the greedy one. 
            bssf = new TSPState(route, bssfCost);
           
        }

        public ArrayList solve(City[] cities, int time_limit)
        {
            //initialize counting variables
            int validRoutesCount = 0;
            int prunedBranches = 0;
            int statesCreated = 0;
            //initialize reults array
            ArrayList results = new ArrayList();
            TSPState currentNode;
            Stopwatch timer = new Stopwatch();
            //initialize priority queue
            BbPriorityQueue pq = new BbPriorityQueue(cities.Length);
            //initialize first TSPState
            currentNode = new TSPState(new ReducedCostMatrix(cities), cities[0], 0, cities.Length);
            statesCreated++;
            //Add it if it's viable. 
            if(currentNode.BssfCost < bssf.BssfCost)
                pq.Add(currentNode);

            timer.Start();
            //go until the pq is empty
            while (!pq.Empty()) 
            {
                //if time runs out, stop
                if (timer.ElapsedMilliseconds > time_limit)
                    break;

                //get the next node
                currentNode = pq.DeleteMin();
                //if the route I pull off is no longer viable, prune it and go to the next one
                if (currentNode.BssfCost >= bssf.BssfCost)
                {
                    prunedBranches++;
                    pq.pruneCurrentPath();
                    continue;
                }
                //if we have visited all the cities
                if (currentNode.route.Count == cities.Length)
                {
                    //make sure we can get to the first city again.
                    double costToStart = currentNode.city.costToGetTo(cities[0]);
                    if (costToStart != double.PositiveInfinity)
                    {
                        validRoutesCount++;
                        //if the total cost is less than the current best, set the currentNode as the current best. 
                        if(currentNode.currentCost + costToStart < bssf.currentCost)
                        {
                            currentNode.currentCost += costToStart;
                            bssf = currentNode;
                        }
                    }
                }

                //create states for every possible city to visit 
                for (int i = 0; i < cities.Length; i++)
                {
                    //skip the loop if we've already visited it on this path
                    if (currentNode.Visited.Contains(i))
                        continue;
                    //make sure we can reach the city
                    double costToI = currentNode.city.costToGetTo(cities[i]);
                    if (!double.PositiveInfinity.Equals(costToI)) { 
                        //create the node
                        TSPState iNode = new TSPState(currentNode, costToI, i, cities[i], cities.Length);
                        statesCreated++;
                        //if it's still viable add it to the queu
                        if (iNode.BssfCost < bssf.BssfCost)
                        {
                            pq.Add(iNode);
                        }
                        else
                        {
                            prunedBranches++;
                        }
                    }
                }
            }
            timer.Stop();
            //set the results
            results.Add(bssf.route);
            results.Add(timer.Elapsed.ToString());
            results.Add(validRoutesCount.ToString());
            return results;
        }

    }

    class TSPState
    {
        //state object to hold the properties that are current to a state
        private ReducedCostMatrix rcm;
        public double currentCost {
            get; set;
        }
        public City city{
            get;
        }
        public HashSet<int> Visited
        {
            get;
        }
        public int Node
        {
            get;
        }
        public ArrayList route
        {
            get;
        }

        public double BssfCost
        {
            get;
        }

        public TSPState(ArrayList route, double bssfCost)
        {
            this.route = route;
            BssfCost = bssfCost;
            currentCost = bssfCost;
        }

        public TSPState(TSPState previousState, double extraCost, int node, City city, int citiesSize)
        {
            Node = node;
            this.city = city;
            currentCost = previousState.currentCost + extraCost;
            Visited = new HashSet<int>(previousState.Visited);
            Visited.Add(node);
            rcm = new ReducedCostMatrix(previousState.rcm, citiesSize);
            BssfCost = rcm.updateRCM(previousState.Node, node, citiesSize) + previousState.BssfCost;
            route = (ArrayList)previousState.route.Clone();
            route.Add(city);
        }

        public TSPState(ReducedCostMatrix rcm, City city, int node, int citiesSize)
        {
            this.rcm = rcm;
            BssfCost = rcm.computeLowerBound(citiesSize);
            this.city = city;
            Node = node;
            Visited = new HashSet<int>();
            Visited.Add(node);
            route = new ArrayList();
            route.Add(city);
        }

    }


    class ReducedCostMatrix
    {
        public double[,] Matrix
        {
            get;
        }

        public ReducedCostMatrix(ReducedCostMatrix rcm, int citiesSize)
        {
           Matrix = new double[citiesSize,citiesSize];
           Array.Copy(rcm.Matrix, Matrix, rcm.Matrix.Length);
        }

        //initialize the rcm 
        public ReducedCostMatrix(City[] cities)
        {
            Matrix = new double[cities.Length, cities.Length];
            //loop through all the rows and columns
            for(int i = 0; i<cities.Length; i++)
            {
                for(int j=0; j<cities.Length; j++)
                {
                    //if i=j, set the value to infinity
                    if (j == i)
                    {
                        Matrix[i, j] = double.PositiveInfinity;
                    }
                    else
                    {//otherwise set the value to the cost to get to that city
                        Matrix[i, j] = cities[i].costToGetTo(cities[j]);
                    }
                }
            }
        }

        public double computeLowerBound(int citiesSize)
        {
            double lowerBound = 0;
            //loop through all the rows and subtract the smallest value to 0
            for (int j = 0; j < citiesSize; j++)
            {
                int smallestEntry = 0;
                //find the smallestEntry
                for (int i = 1; i < citiesSize; i++)
                {
                    if (Matrix[i, j] < Matrix[smallestEntry, j])
                    {
                        smallestEntry = i;
                    }
                }
                //subtract it from each position in the row, and add the value to the lowerBound
                double entryValue = Matrix[smallestEntry, j];
                if (!double.PositiveInfinity.Equals(entryValue))
                {
                    lowerBound += entryValue;
                    for (int i = 0; i < citiesSize; i++)
                    {
                        Matrix[i, j] -= entryValue;
                    }
                }
            }
            //columns, same thing as rows.
            for(int i=0; i<citiesSize; i++)
            {
                int smallestEntry = 0;
                for (int j = 1; j < citiesSize; j++)
                {
                    if (Matrix[i, j] < Matrix[i, smallestEntry])
                    {
                        smallestEntry = j;
                    }
                }
                double entryValue = Matrix[i, smallestEntry];
                if (!double.PositiveInfinity.Equals(entryValue))
                {
                    lowerBound += entryValue;
                    for (int j = 0; j < citiesSize; j++)
                    {
                        Matrix[i, j] -= entryValue;
                    }
                }
            }
            return lowerBound;
        }

        public double updateRCM(int startIndex, int endIndex, int citiesSize)
        {
            //set all values along row and column to be infinity, then compute the lowerBound again. 
            double extraCost = Matrix[startIndex, endIndex];
            for(int i=0; i<citiesSize; i++)
            {
                Matrix[i, endIndex] = double.PositiveInfinity;
            }
            for(int j=0; j< citiesSize; j++)
            {
                Matrix[startIndex, j] = double.PositiveInfinity;
            }
            return computeLowerBound(citiesSize) + extraCost;
        }

    }



    
}
