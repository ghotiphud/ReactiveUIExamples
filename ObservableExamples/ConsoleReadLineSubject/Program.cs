using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace ConsoleReadLine
{
    class Program
    {
        static Subject<int> subject = new Subject<int>();

        static void Main(string[] args)
        {
            // a
            var a = 0;
            subject
                .Where(i => i % 2 == 0)
                .Subscribe(
                    onNext: i => { a += i; Console.WriteLine("a: {0}", a); },
                    onCompleted: () => Console.WriteLine("a: {0}", a));

            // b
            subject
                .Where(i => i > 5)
                .Sum()
                .Subscribe(b => Console.WriteLine("b: {0}", b));

            // c
            subject
                .Where(i => i < 10)
                .Sum()
                .Subscribe(c => Console.WriteLine("c: {0}", c));

            { // Read every line from the console and publish to the subject.
                var line = "";
                var input = 0;
                while (true)
                {
                    line = Console.ReadLine();
                    if (int.TryParse(line, out input))
                    {
                        subject.OnNext(input);
                    }

                    if (line == "exit")
                    {
                        subject.OnCompleted();
                        break;
                    }
                }
            }

            Console.ReadLine();
        }
    }
}
