using Counters.database;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;
using System.Threading.Tasks;

namespace Counters
{
    internal class Program
    {
        private static int count { get; set; } = 0;
        public static int TimeChange { get; set; } = 5;
        public static int CountCounters { get; set; } = 1;
        const string token = "CvZq-BeU7hnXLQ-EPK1u0PnXoEVnDtt4eP_r1rpFr7ErcdElMRzpbuBwMjStCAr1-2VJykjlF9eH8hJszICXFw==";
        //const string bucket = "test";
        const string org = "IRZ";

        static InfluxDBClient? client = null;
            
        static async Task Main(string[] args)
        {
            client = InfluxDBClientFactory.Create("http://localhost:8086", token);

            using (databaseContext db = new databaseContext())
            {
                //var counters = db.Counters.Select(p => p);
                //if (counters.Count() == 0)
                //{
                //    InitCounters(CountCounters);
                //}

                InitCountersAsync(CountCounters);

                //TimerCallback timerCallback = new TimerCallback(ChangeCountersAsync);
                //Timer timer = new Timer(timerCallback, count, 0, TimeChange * 1000);

                //Console.WriteLine("Нажмите ESC для остановки программмы");
                //while (true)
                //{
                //    if (Console.ReadKey().Key == ConsoleKey.Escape)
                //        break;
                //}
            }
        }

        public static async Task InitCountersAsync(int count)
        {
            //using (databaseContext db = new databaseContext())
            //{
            //    var rand = new Random();
            //    for (int i = 0; i < count; i++)
            //    {
            //        Counter counter = new Counter();

            //        var indicators = SetIndicators();
            //        counter.Voltage = indicators.voltage;
            //        counter.AC = indicators.ac;
            //        counter.ReactEnergy = indicators.reactEnergy;
            //        counter.ActiveEnergy = indicators.activeEnergy;

            //        db.Counters.Add(counter);
            //    }
            //    db.SaveChanges();                    
            //}

            using (var writeApi = client.GetWriteApi())
            {
                BucketRetentionRules bucketRetentionRules = new BucketRetentionRules(type: BucketRetentionRules.TypeEnum.Expire, everySeconds: 3600);
                var orgId = (await client.GetOrganizationsApi().FindOrganizationsAsync(org: org)).First().Id;

                for (int i = 0; i < CountCounters; i++)
                {
                    var bucket = await client.GetBucketsApi().CreateBucketAsync((i + 1).ToString(), bucketRetentionRules, orgId);
                    //var resource = new PermissionResource(PermissionResource.TypeBuckets, bucket.Id, null, orgId);

                    //var write = new Permission(Permission.ActionEnum.Write, resource);
                    //var read = new Permission(Permission.ActionEnum.Read, resource);

                    //var authorization = await client.GetAuthorizationsApi().CreateAuthorizationAsync(orgId, new List<Permission> { read, write });


                    var indicators = SetIndicators();

                    var counter = new Count();
                    counter.Voltage = indicators.voltage;
                    counter.AC = indicators.ac;
                    counter.ReactEnergy = indicators.reactEnergy;
                    counter.ActiveEnergy = indicators.activeEnergy;

                    writeApi.WriteMeasurement(counter, WritePrecision.Ns, bucket.Name, org);
                }

                ChangeCountersAsync(new object());
            }
        }

        private static async void ChangeCountersAsync(object obj)
        {
            count++;

            Console.WriteLine($"Изменение данных в базе данных - {count}");

            //using (databaseContext db = new databaseContext())
            //{
            //    var rand = new Random();
            //    var counters = db.Counters.Select(p => p);

            //    foreach (var counter in counters)
            //    {
            //        var newIndicators = SetIndicators();
            //        counter.Voltage = newIndicators.voltage;
            //        counter.AC = newIndicators.ac;
            //        counter.ReactEnergy = newIndicators.reactEnergy;
            //        counter.ActiveEnergy = newIndicators.activeEnergy;
            //    }

            //    db.SaveChanges();
            //}

            using (var writeApi = client.GetWriteApi())
            {
                for (int i = 0; i < CountCounters; i++)
                {
                    var query = $"from(bucket: \"{i + 1}\") |> range(start: 0)";
                    var tables = await client.GetQueryApi().QueryAsync(query, org);

                    foreach (var record in tables.SelectMany(table => table.Records))
                    {
                        Console.WriteLine($"{record}");
                    }
                }
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