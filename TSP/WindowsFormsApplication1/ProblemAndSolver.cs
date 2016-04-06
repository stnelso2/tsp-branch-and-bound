using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;


namespace TSP
{

    class ProblemAndSolver
    {

        public class TSPSolution: IComparable
        {


            public TSPSolution(TSPSolution solution)
            {
                this.Route = (ArrayList)solution.Route.Clone();
                this.cost = solution.cost;
            }
            /// <summary>
            /// we use the representation [cityB,cityA,cityC] 
            /// to mean that cityB is the first city in the solution, cityA is the second, cityC is the third 
            /// and the edge from cityC to cityB is the final edge in the path.  
            /// You are, of course, free to use a different representation if it would be more convenient or efficient 
            /// for your data structure(s) and search algorithm. 
            /// </summary>
            public ArrayList
                Route;

            public double cost
            {
                get; set;
            }

            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="iroute">a (hopefully) valid tour</param>
            public TSPSolution(ArrayList iroute)
            {
                Route = new ArrayList(iroute);
                cost = costOfRoute();
            }

            /// <summary>
            /// Compute the cost of the current route.  
            /// Note: This does not check that the route is complete.
            /// It assumes that the route passes from the last city back to the first city. 
            /// </summary>
            /// <returns></returns>
            public double costOfRoute()
            {
                
                // go through each edge in the route and add up the cost. 
                int x;
                City here;
                cost = 0D;

                for (x = 0; x < Route.Count - 1; x++)
                {
                    here = Route[x] as City;
                    cost += here.costToGetTo(Route[x + 1] as City);
                }

                // go from the last city to the first. 
                here = Route[Route.Count - 1] as City;
                cost += here.costToGetTo(Route[0] as City);
                return cost;
            }

            public int CompareTo(Object obj)
            {
                if (obj == null) return 1;

                TSPSolution otherSoln = obj as TSPSolution;

                if(otherSoln != null)
                {
                    return this.cost.CompareTo(otherSoln.cost);
                }
                else
                {
                    throw new ArgumentException("Object is not a TSPSolution");
                }
                
            }
        }

        #region Private members 

        /// <summary>
        /// Default number of cities (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Problem Size text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int DEFAULT_SIZE = 25;

        /// <summary>
        /// Default time limit (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Time text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int TIME_LIMIT = 60;        //in seconds

        private const int CITY_ICON_SIZE = 5;


        // For normal and hard modes:
        // hard mode only
        private const double FRACTION_OF_PATHS_TO_REMOVE = 0.20;

        /// <summary>
        /// the cities in the current problem.
        /// </summary>
        private City[] Cities;
        /// <summary>
        /// a route through the current problem, useful as a temporary variable. 
        /// </summary>
        private ArrayList Route;
        /// <summary>
        /// best solution so far. 
        /// </summary>
        private TSPSolution bssf; 

        /// <summary>
        /// how to color various things. 
        /// </summary>
        private Brush cityBrushStartStyle;
        private Brush cityBrushStyle;
        private Pen routePenStyle;


        /// <summary>
        /// keep track of the seed value so that the same sequence of problems can be 
        /// regenerated next time the generator is run. 
        /// </summary>
        private int _seed;
        /// <summary>
        /// number of cities to include in a problem. 
        /// </summary>
        private int _size;

        /// <summary>
        /// Difficulty level
        /// </summary>
        private HardMode.Modes _mode;

        /// <summary>
        /// random number generator. 
        /// </summary>
        private Random rnd;

        /// <summary>
        /// time limit in milliseconds for state space search
        /// can be used by any solver method to truncate the search and return the BSSF
        /// </summary>
        private int time_limit;
        #endregion

        #region Public members

        /// <summary>
        /// These three constants are used for convenience/clarity in populating and accessing the results array that is passed back to the calling Form
        /// </summary>
        public const int COST = 0;           
        public const int TIME = 1;
        public const int COUNT = 2;
        
        public int Size
        {
            get { return _size; }
        }

        public int Seed
        {
            get { return _seed; }
        }
        #endregion

        #region Constructors
        public ProblemAndSolver()
        {
            this._seed = 1; 
            rnd = new Random(1);
            this._size = DEFAULT_SIZE;
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }

        public ProblemAndSolver(int seed)
        {
            this._seed = seed;
            rnd = new Random(seed);
            this._size = DEFAULT_SIZE;
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }

        public ProblemAndSolver(int seed, int size)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = TIME_LIMIT * 1000;                        // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }
        public ProblemAndSolver(int seed, int size, int time)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = time*1000;                        // time is entered in the GUI in seconds, but timer wants it in milliseconds

            this.resetData();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Reset the problem instance.
        /// </summary>
        private void resetData()
        {

            Cities = new City[_size];
            Route = new ArrayList(_size);
            bssf = null;

            if (_mode == HardMode.Modes.Easy)
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble());
            }
            else // Medium and hard
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble() * City.MAX_ELEVATION);
            }

            HardMode mm = new HardMode(this._mode, this.rnd, Cities);
            if (_mode == HardMode.Modes.Hard)
            {
                int edgesToRemove = (int)(_size * FRACTION_OF_PATHS_TO_REMOVE);
                mm.removePaths(edgesToRemove);
            }
            City.setModeManager(mm);

            cityBrushStyle = new SolidBrush(Color.Black);
            cityBrushStartStyle = new SolidBrush(Color.Red);
            routePenStyle = new Pen(Color.Blue,1);
            routePenStyle.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode)
        {
            this._size = size;
            this._mode = mode;
            resetData();
        }

        /// <summary>
        /// make a new problem with the given size, now including timelimit paremeter that was added to form.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode, int timelimit)
        {
            this._size = size;
            this._mode = mode;
            this.time_limit = timelimit*1000;                                   //convert seconds to milliseconds
            resetData();
        }

        /// <summary>
        /// return a copy of the cities in this problem. 
        /// </summary>
        /// <returns>array of cities</returns>
        public City[] GetCities()
        {
            City[] retCities = new City[Cities.Length];
            Array.Copy(Cities, retCities, Cities.Length);
            return retCities;
        }

        /// <summary>
        /// draw the cities in the problem.  if the bssf member is defined, then
        /// draw that too. 
        /// </summary>
        /// <param name="g">where to draw the stuff</param>
        public void Draw(Graphics g)
        {
            float width  = g.VisibleClipBounds.Width-45F;
            float height = g.VisibleClipBounds.Height-45F;
            Font labelFont = new Font("Arial", 10);

            // Draw lines
            if (bssf != null)
            {
                // make a list of points. 
                Point[] ps = new Point[bssf.Route.Count];
                int index = 0;
                foreach (City c in bssf.Route)
                {
                    if (index < bssf.Route.Count -1)
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[index+1]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    else 
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[0]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    ps[index++] = new Point((int)(c.X * width) + CITY_ICON_SIZE / 2, (int)(c.Y * height) + CITY_ICON_SIZE / 2);
                }

                if (ps.Length > 0)
                {
                    g.DrawLines(routePenStyle, ps);
                    g.FillEllipse(cityBrushStartStyle, (float)Cities[0].X * width - 1, (float)Cities[0].Y * height - 1, CITY_ICON_SIZE + 2, CITY_ICON_SIZE + 2);
                }

                // draw the last line. 
                g.DrawLine(routePenStyle, ps[0], ps[ps.Length - 1]);
            }

            // Draw city dots
            foreach (City c in Cities)
            {
                g.FillEllipse(cityBrushStyle, (float)c.X * width, (float)c.Y * height, CITY_ICON_SIZE, CITY_ICON_SIZE);
            }

        }

        /// <summary>
        ///  return the cost of the best solution so far. 
        /// </summary>
        /// <returns></returns>
        public double costOfBssf ()
        {
            if (bssf != null)
                return (bssf.costOfRoute());
            else
                return -1D; 
        }

        public string[] defaultSolveProblem()
        {
            return defaultSolveProblem(new SortedSet<TSPSolution>());
        }

        /// <summary>
        /// This is the entry point for the default solver
        /// which just finds a valid random tour 
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] defaultSolveProblem(SortedSet<TSPSolution> population)
        {
            int i, swap, temp, count=0;
            string[] results = new string[3];
            int[] perm = new int[Cities.Length];
            Route = new ArrayList();
            Random rnd = new Random();
            Stopwatch timer = new Stopwatch();

            timer.Start();

            do
            {
                for (i = 0; i < perm.Length; i++)                                 // create a random permutation template
                    perm[i] = i;
                for (i = 0; i < perm.Length; i++)
                {
                    swap = i;
                    while (swap == i)
                        swap = rnd.Next(0, Cities.Length);
                    temp = perm[i];
                    perm[i] = perm[swap];
                    perm[swap] = temp;
                }
                Route.Clear();
                for (i = 0; i < Cities.Length; i++)                            // Now build the route using the random permutation 
                {
                    Route.Add(Cities[perm[i]]);
                }
                bssf = new TSPSolution(Route);
                count++;
            } while (costOfBssf() == double.PositiveInfinity);                // until a valid route is found
            timer.Stop();
            population.Add(bssf);

            results[COST] = costOfBssf().ToString();                          // load results array
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = count.ToString();

            return results;
        }

        /// <summary>
        /// performs a Branch and Bound search of the state space of partial tours
        /// stops when time limit expires and uses BSSF as solution
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] bBSolveProblem()
        {
            string[] results = new string[3];

            // TODO: Add your implementation for a branch and bound solver here.
            greedySolveProblem();//solve the initial path with the greedy solver
            BbSolver solver = new BbSolver(Cities, bssf.Route, bssf.costOfRoute());//pass in the greedy route with the cost and the cities.
            ArrayList solution = solver.solve(Cities, time_limit);//solves the problem with branch and bound
            //set results.
            Route = (ArrayList)solution[0];
            bssf = new TSPSolution(Route);
            results[COST] = bssf.costOfRoute().ToString();   
            results[TIME] = (String) solution[1];
            results[COUNT] = (String) solution[2];

            return results;
        }

        public string[] greedySolveProblem()
        {
            return greedySolveProblem(new SortedSet<TSPSolution>());
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        // These additional solver methods will be implemented as part of the group project.
        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// finds the greedy tour starting from each city and keeps the best (valid) one
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] greedySolveProblem(SortedSet<TSPSolution> population)
        {
            string[] results = new string[3];
            bssf = null;
            double bssfCost = Double.PositiveInfinity;
            int validRouteCount = 0;
            Stopwatch timer = new Stopwatch();

            timer.Start();
            //loop through every city, and use it as a starting point
            for (int i=0; i<Cities.Length; i++)
            {
                if (timer.ElapsedMilliseconds > time_limit)
                    break;
                //initialize varialbe
                HashSet<int> visited = new HashSet<int>();
                visited.Add(i);
                int start = i;
                int current = i;
                Route = new ArrayList();
                //add the current city
                Route.Add(Cities[current]);
                bool validRoute = true;
                //start the while loop to find a complete path for the starting city
                do
                {
                    int currentBest = -1;
                    double currentBestCost = Double.PositiveInfinity;
                    //find the best cost destination
                    for (int j = 0; j < Cities.Length; j++)
                    {
                        if (visited.Contains(j))
                            continue;
                        double costToJ = Cities[current].costToGetTo(Cities[j]);
                        if(costToJ < currentBestCost)
                        {
                            currentBest = j;
                            currentBestCost = costToJ;
                        }
                    }
                    //if it didn't find a city to visit
                    if(currentBest == -1)
                    {
                        break;
                        validRoute = false;
                    }
                    else
                    {//if it did find a city to visit
                        current = currentBest;
                        Route.Add(Cities[current]);
                        visited.Add(current);
                    }


                } while (Route.Count < Cities.Length);
                //if the starting city is not reachable
                if(Cities[current].costToGetTo(Cities[start]) == double.PositiveInfinity)
                {
                    validRoute = false;
                }
                if (validRoute)
                {//if it found a valid route, compare it to the current one and swap if it's better
                    validRouteCount++;
                    if (bssf == null)
                    {
                        bssf = new TSPSolution(Route);
                        population.Add(bssf);
                    }
                    else
                    {//
                        TSPSolution currentSolution = new TSPSolution(Route);
                        population.Add(currentSolution);
                        double currentSolutionCost = currentSolution.costOfRoute();
                        //swap if the current solution is better
                        if(currentSolutionCost < bssfCost)
                        {
                            bssf = currentSolution;
                            bssfCost = currentSolutionCost;
                        }
                    }
                }
            }
            timer.Stop();

            results[COST] = bssfCost.ToString();    // load results into array 
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = validRouteCount.ToString();

            return results;
        }




        public string[] fancySolveProblem()
        {
            string[] results = new string[3];
            int populationCount = 50;
            SortedSet<TSPSolution> population = new SortedSet<TSPSolution>();
            greedySolveProblem(population);
            while(population.Count < populationCount)
            {
                defaultSolveProblem(population);
            }
            Stopwatch timer = new Stopwatch();
            timer.Start();

            //bool done = false;
            while (timer.ElapsedMilliseconds < time_limit)
            {
                TSPSolution[] parents = chooseParents(population);
                TSPSolution[] children = crossover(parents);
                population.Add(children[0]);
                population.Add(children[1]);
                mutation(population);
                prune(population, populationCount);
            }
            timer.Stop();
            
            foreach(TSPSolution solution in population)
            {
                if(solution.cost < bssf.cost)
                {
                    bssf = solution;
                }
            }

            

            results[COST] = bssf.cost.ToString();    // load results into array here, replacing these dummy values
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = "-1";

            return results;
        }

        private TSPSolution[] chooseParents(SortedSet<TSPSolution> population)
        {
            TSPSolution[] parents = new TSPSolution[2];
            Random random = new Random();
            int parent = 0;
            foreach(TSPSolution solution in population)
            {
                //if(random.Next(4) != 0)
                //{
                    parents[parent] = solution;
                    parent++;
                    if (parent == 2) break;
                //}
            }

            return parents;
        }

        private TSPSolution[] crossover(TSPSolution[] parents)
        {
            TSPSolution[] children = new TSPSolution[2];
            Random random = new Random();
            bool valid = false;
            while (!valid)
            {
                int index1 = random.Next(Cities.Length);
                int index2 = random.Next(Cities.Length);
                while (index2 == index1)
                {
                    index2 = random.Next(Cities.Length);
                }
                if(index2 < index1)
                {
                    int temp = index1;
                    index1 = index2;
                    index2 = index1;
                }

                TSPSolution parent1 = parents[0];
                TSPSolution parent2 = parents[1];
                City[] child1 = new City[Cities.Length];
                City[] child2 = new City[Cities.Length];
                HashSet<City> visited1 = new HashSet<City>();
                HashSet<City> visited2 = new HashSet<City>();
                for(int i=index1; i<= index2; i++)
                {
                    child1[i] = (City)parent1.Route[i];
                    visited1.Add(child1[i]);
                    child2[i] = (City)parent2.Route[i];
                    visited2.Add(child2[i]);
                }
                int child1Index = 0;
                int child2Index = 0;
                for(int i=0; i<Cities.Length; i++)
                {
                    if (child1Index == index1)
                    {
                        child1Index = index2 + 1;
                    }
                    if(!visited1.Contains((City)parent2.Route[i]))
                    {
                        child1[child1Index] = (City)parent2.Route[i];
                        child1Index++;
                    }

                    if(child2Index == index1)
                    {
                        child2Index = index2 + 1;
                    }
                    if (!visited2.Contains((City)parent1.Route[i]))
                    {
                        child2[child2Index] = (City)parent1.Route[i];
                        child2Index++;
                    }
                }

                TSPSolution child1Solution = new TSPSolution(new ArrayList(child1));
                TSPSolution child2Solution = new TSPSolution(new ArrayList(child2));
                
                if (child1Solution.cost != double.PositiveInfinity && child2Solution.cost != double.PositiveInfinity)
                {
                    valid = true;
                    children[0] = child1Solution;
                    children[1] = child2Solution;
                }
            }
            return children;
        }

        private void mutation(SortedSet<TSPSolution> population)
        {
            Random random = new Random();
            foreach(TSPSolution solution in population)
            {
                if(random.Next(population.Count) == 0)
                {
                    mutate(solution);
                }
            }
        }

        private void mutate(TSPSolution solution)
        {
            bool valid = false;
            Random random = new Random();
            TSPSolution mutation  = null;
            while (!valid)
            {
                int index1 = random.Next(Cities.Length);
                int index2 = random.Next(Cities.Length);
                while(index1 == index2)
                {
                    index2 = random.Next(Cities.Length);
                }
                City temp = (City)solution.Route[index1];
                solution.Route[index1] = solution.Route[index2];
                solution.Route[index2] = temp;
                if(solution.costOfRoute() != double.PositiveInfinity)
                {
                    valid = true;
                }
                else
                {
                    temp = (City)solution.Route[index1];
                    solution.Route[index1] = solution.Route[index2];
                    solution.Route[index2] = temp;
                }
            }
        }

        private void prune(SortedSet<TSPSolution> population, int populationCount)
        {
            List<TSPSolution> toRemoveList = new List<TSPSolution>();
            Random random = new Random();
            foreach(TSPSolution solution in population.Reverse())
            {
                //if(random.Next(4) != 0)
                //{
                    toRemoveList.Add(solution);
                //}
                if (population.Count - toRemoveList.Count <= populationCount) break;
            }
            foreach(TSPSolution solution in toRemoveList)
            {
                population.Remove(solution);
            }
        }

       
        #endregion
    }

}
