using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleObservable
{
    class Program
    {
        static Stopwatch stopwatch = new Stopwatch();

        static void Main(string[] args)
        {
            var list = new List<int> { 1, 2, 5, 6, 7, 9, 3, 1, 56, 9, 3, 12 };
            stopwatch.Start();
            Enumerable_Blocking(GetHeavyEnumerable(list));
            Console.WriteLine("time: {0}", stopwatch.Elapsed);

            stopwatch.Restart();
            Console.WriteLine("time restarted");
            Observable_NonBlocking(GetHeavyEnumerable(list));
            Console.WriteLine("time: {0}", stopwatch.Elapsed);

            Console.ReadKey();
        }

        static void Enumerable_Blocking(IEnumerable<int> list)
        {
            int a = 0;
            int b = 0;
            int c = 0;

            foreach (var i in list)
            {
                if (i % 2 == 0) a += i;
                if (i > 5) b += i;
                if (i < 10) c += i;
            }

            Console.WriteLine("a: {0}", a);
            Console.WriteLine("b: {0}", b);
            Console.WriteLine("c: {0}", c);
        }

        static async void Observable_NonBlocking(IEnumerable<int> list)
        {
            var observable = list.ToObservable(Scheduler.Default);

            // a
            // subscribe overload with onNext and onComplete.
            var a = 0;
            observable
                .Where(i => i % 2 == 0)
                .Subscribe(
                    onNext: i => a += i,
                    onCompleted: () => Console.WriteLine("a: {0}", a));

            // b
            // Sum keeps a running total until the onComplete call.
            observable
                .Where(i => i % 2 == 1)
                .Sum()
                .Subscribe(b => Console.WriteLine("b: {0}", b));

            // c
            // show how observables can be awaited.
            var obc = observable
                .Where(i => i < 10)
                .Sum();

            Console.WriteLine("c: {0}", await obc.LastAsync());
            Console.WriteLine("time: {0}", stopwatch.Elapsed);
        }

        static IEnumerable<int> GetHeavyEnumerable(IEnumerable<int> list)
        {
            foreach (var i in list)
            {
                Thread.Sleep(200);
                yield return i;
            }
        }
    }
}
