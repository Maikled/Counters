using InfluxDB.Client;
using InfluxDB.Client.Core;

namespace Counters
{
    internal class Program
    {
        private static int countChange { get; set; } = 0;
        public static int TimeChange { get; set; } = 5;
        public static int CountCounters { get; set; } = 2;

        const string token = "jRHApZdHnvpXkv4vLzQXwSWIRhdhbKWBTiHIotQw5VglRK5bBInPfmdUFG8ejLep6dECiX44rOToIh_dYBgzYQ==";
        const string org = "IRZ";
        const string bucket = "data";

        static InfluxDBClient? client;
            
        static async Task Main(string[] args)
        {
            var options = new InfluxDBClientOptions.Builder()
                .Url("http://localhost:8086")
                .AuthenticateToken(token.ToCharArray())
                .Org(org)
                .Bucket(bucket)
                .Build();

            client = InfluxDBClientFactory.Create(options);
            client.SetLogLevel(LogLevel.Body);

            while (true)
            {
                WriteDataCounters(CountCounters); 
                Thread.Sleep(1000 * TimeChange);
            }
        }

        public static void WriteDataCounters(int count)
        {
            Console.WriteLine($"Начало записи данных счётчиков в {bucket} - {countChange}");

            for (int i = 0; i < count; i++)
            {
                using (var writeApi = client.GetWriteApi())
                {
                    var indicators = SetIndicators();

                    var counter = new Count();
                    counter.Id = i;
                    counter.Voltage = indicators.voltage;
                    counter.AC = indicators.ac;
                    counter.ReactEnergy = indicators.reactEnergy;
                    counter.ActiveEnergy = indicators.activeEnergy;
                    counter.Time = DateTime.UtcNow;

                    writeApi.WriteMeasurement(counter);
                }
            }

            Console.WriteLine($"Окончание записи данных счётчиков в {bucket} - {countChange}\n");
            countChange++;
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


        [Measurement("count")]
        private class Count
        {
            [Column("ID", IsTag = true)] public int Id { get; set; }
            [Column("Voltage")] public double Voltage { get; set; }
            [Column("AC")] public double AC { get; set; }
            [Column("ReactEnergy")] public double ReactEnergy { get; set; }
            [Column("ActiveEnergy")] public double ActiveEnergy { get; set; }
            [Column(IsTimestamp = true)] public DateTime Time { get; set; }
        }
    }
}