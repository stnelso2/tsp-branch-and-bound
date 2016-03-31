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
        //need a priority queue of states. 
        //need a reduced cost matrix
        //need a state object 
        ReducedCostMatrix rcm;
        TSPState bssf = null;

        public BbSolver(City[] cities, ArrayList route, double bssfCost)
        {
            bssf = new TSPState(route, bssfCost);
           
        }

        public ArrayList solve(City[] cities, int time_limit)
        {
            int validRoutesCount = 0;
            ArrayList results = new ArrayList();
            TSPState currentNode;
            Stopwatch timer = new Stopwatch();
            BbPriorityQueue pq = new BbPriorityQueue(cities.Length);
            currentNode = new TSPState(new ReducedCostMatrix(cities), cities[0], 0, cities.Length);
            if(currentNode.BssfCost < bssf.BssfCost)
                pq.Add(currentNode);

            timer.Start();
            while (!pq.Empty()) 
            {
                if (timer.ElapsedMilliseconds > time_limit)
                    break;

                currentNode = pq.DeleteMin();
                if (currentNode.BssfCost >= bssf.BssfCost)
                {
                    pq.pruneCurrentPath();
                    continue;
                }
                if (currentNode.route.Count == cities.Length)
                {
                    double costToStart = currentNode.city.costToGetTo(cities[0]);
                    if (costToStart != double.PositiveInfinity)
                    {
                        validRoutesCount++;
                        if(currentNode.currentCost + costToStart < bssf.currentCost)
                        {
                            currentNode.currentCost += costToStart;
                            bssf = currentNode;
                        }
                    }
                }

                for (int i = 0; i < cities.Length; i++)
                {
                    if (currentNode.Visited.Contains(i))
                        continue;
                    double costToI = currentNode.city.costToGetTo(cities[i]);
                    if (!double.PositiveInfinity.Equals(costToI)) { 
                        TSPState iNode = new TSPState(currentNode, costToI, i, cities[i], cities.Length);
                        if (iNode.BssfCost < bssf.BssfCost)
                        {
                            pq.Add(iNode);
                        }
                    }
                }
            }
            timer.Stop();
            results.Add(bssf.route);
            results.Add(timer.Elapsed.ToString());
            results.Add(validRoutesCount.ToString());
            return results;
        }

    }

    class TSPState
    {
        private ReducedCostMatrix rcm;
        public string parentPath {
            get;
        }
        private TSPState previousState;
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
        public int Length
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
            this.previousState = previousState;
            parentPath = previousState.parentPath + previousState.Node;
            this.city = city;
            currentCost = previousState.currentCost + extraCost;
            Visited = new HashSet<int>(previousState.Visited);
            Visited.Add(node);
            rcm = new ReducedCostMatrix(previousState.rcm, citiesSize);
            BssfCost = rcm.updateRCM(previousState.Node, node, citiesSize) + previousState.BssfCost;
            Length = previousState.Length + 1;
            route = (ArrayList)previousState.route.Clone();
            route.Add(city);
        }

        public TSPState(ReducedCostMatrix rcm, City city, int node, int citiesSize)
        {
            this.rcm = rcm;
            BssfCost = rcm.computeLowerBound(citiesSize);
            previousState = null;
            parentPath = "";
            this.city = city;
            Node = node;
            Visited = new HashSet<int>();
            Visited.Add(node);
            Length = 1;
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

        public ReducedCostMatrix(City[] cities)
        {
            Matrix = new double[cities.Length, cities.Length];
            for(int i = 0; i<cities.Length; i++)
            {
                for(int j=0; j<cities.Length; j++)
                {
                    if (j == i)
                    {
                        Matrix[i, j] = double.PositiveInfinity;
                    }
                    else
                    {
                        Matrix[i, j] = cities[i].costToGetTo(cities[j]);
                    }
                }
            }
        }

        public double computeLowerBound(int citiesSize)
        {
            double lowerBound = 0;
            for (int j = 0; j < citiesSize; j++)
            {
                int smallestEntry = 0;
                for (int i = 1; i < citiesSize; i++)
                {
                    if (Matrix[i, j] < Matrix[smallestEntry, j])
                    {
                        smallestEntry = i;
                    }
                }
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
            //columns
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
