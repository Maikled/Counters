using Counters.database;

namespace Counters
{
    internal class Program
    {
        private static int count { get; set; } = 0;
        public static int TimeChange { get; set; } = 5;
        public static int CountCounters { get; set; } = 1000;

        static void Main(string[] args)
        {
            using (databaseContext db = new databaseContext())
            {
                var counters = db.Counters.Select(p => p);
                if (counters.Count() == 0)
                {
                    InitCounters(CountCounters);
                }

                TimerCallback timerCallback = new TimerCallback(ChangeCounters);
                Timer timer = new Timer(timerCallback, count, 0, TimeChange * 1000);

                Console.WriteLine("Нажмите ESC для остановки программмы");
                while (true)
                {
                    if (Console.ReadKey().Key == ConsoleKey.Escape)
                        break;
                }
            }
        }

        public static void InitCounters(int count)
        {
            using (databaseContext db = new databaseContext())
            {
                var rand = new Random();
                for (int i = 0; i < count; i++)
                {
                    Counter counter = new Counter();

                    var indicators = SetIndicators();
                    counter.Voltage = indicators.voltage;
                    counter.AC = indicators.ac;
                    counter.ReactEnergy = indicators.reactEnergy;
                    counter.ActiveEnergy = indicators.activeEnergy;

                    db.Counters.Add(counter);
                }
                db.SaveChanges();                    
            }
        }

        private static void ChangeCounters(object obj)
        {
            count++;

            Console.WriteLine($"Изменение данных в базе данных - {count}");

            using (databaseContext db = new databaseContext())
            {
                var rand = new Random();
                var counters = db.Counters.Select(p => p);

                foreach (var counter in counters)
                {
                    var newIndicators = SetIndicators();
                    counter.Voltage = newIndicators.voltage;
                    counter.AC = newIndicators.ac;
                    counter.ReactEnergy = newIndicators.reactEnergy;
                    counter.ActiveEnergy = newIndicators.activeEnergy;
                }

                db.SaveChanges();
            }

            Console.WriteLine($"Изменение данных успешно завершено - {count}\n");
        }

        private static Indicators SetIndicators(int voltageStart = 180, int voltageEnd = 380, int acStart = 5, int acEnd = 50, int reactEnergyStart = 1, int reactEnergyEnd = 100, int activeEnergyStart = 1, int activeEnergyEnd = 100)
        {
            var rand = new Random();

            var indicators = new Indicators();
            indicators.voltage = rand.Next(voltageStart, voltageEnd);
            indicators.ac = rand.Next(acStart, acEnd);
            indicators.reactEnergy = rand.Next(reactEnergyStart, reactEnergyEnd);
            indicators.activeEnergy = rand.Next(activeEnergyStart, activeEnergyEnd);

            return indicators;
        }

        private struct Indicators
        { 
            public double voltage;
            public double ac;
            public double reactEnergy;
            public double activeEnergy;
        }
    }
}