using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFizzBuzz
{
    class Program
    {
        static Subject<int> subject = new Subject<int>();

        static void Main(string[] args)
        {
            subject.Subscribe(i => Console.WriteLine("out: " + FizzBuzz(i)));

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

        private static string FizzBuzz(int i)
        {
            var fizz = i % 3 == 0;
            var buzz = i % 5 == 0;

            var result = "";
            if (fizz) result += "Fizz";
            if (buzz) result += "Buzz";
            if (result == "") result += i.ToString();

            return result;
        }
    }
}
;