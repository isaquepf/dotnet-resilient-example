using System;
using Polly;
using System.Threading.Tasks;
using System.Timers;

namespace resilient_example
{
  class Program
  {
    public static Timer Timer => new Timer()
    {
      Interval = TimeSpan.FromSeconds(10).TotalMilliseconds,
      Enabled = true,
      AutoReset = true
    };
    private static int _count = 1;
    static void Main(string[] args)
    {
      MainAsync().Wait();
    }
    static async Task MainAsync()
    {
      await Retry();
      Console.ReadKey();
    }

    public static async Task Retry()
    {
      Console.WriteLine("Hello World!");

      var policy = Policy.Handle<ArgumentException>(
          exception =>
          {
            Console.WriteLine(exception.Message);
            return true;
          }
      ).WaitAndRetryAsync(5, time => TimeSpan.FromSeconds(10));

      await policy.ExecuteAsync(async () =>
      {
        await DoFakeRequest();
      });
    }

    public static async Task DoFakeRequest()
    {
      Timer.Elapsed += OnTimeEvent;

      await Task.Factory.StartNew(() =>
      {
        Console.WriteLine($"Req retry Number:{_count++}");
        throw new ArgumentException("Service fault");
      });
    }

    public static void OnTimeEvent(Object source, ElapsedEventArgs e)
        => Console.WriteLine($"Elapsed on {e.SignalTime}");
  }
}

