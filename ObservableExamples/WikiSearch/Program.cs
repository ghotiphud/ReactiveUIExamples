using LinqToWiki;
using LinqToWiki.Generated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WikiSearch
{
    class Program
    {
        static ConsoleColor defaultColor = Console.ForegroundColor;
        static Subject<string> searchStrings = new Subject<string>();
        static Subject<SearchResult> searchResults = new Subject<SearchResult>();
        static Wiki wiki = new Wiki("LinqToWiki UserAgent", "en.wikipedia.org");

        static void Main(string[] args)
        {
            // Heavy operation that we want to occur on a separate thread.
            searchStrings
                .ObserveOn(Scheduler.Default)
                .Subscribe(s => SearchWiki(s));

            searchResults
                .Subscribe(r => Write(r));

            { // Read every line from the console and publish to the subject.
                var line = "";
                while (true)
                {
                    Console.WriteLine("Enter Search Term: ");
                    line = Console.ReadLine();

                    if (line == "exit")
                    {
                        searchStrings.OnCompleted();
                        break;
                    }
                    else
                    {
                        searchStrings.OnNext(line);
                    }
                }
            }

            Console.ReadLine();
        }

        static void SearchWiki(string searchString)
        {
            // Just in case the search isn't slow enough for illustration
            //Thread.Sleep(2000);

            var pageTitles = wiki.Query.search(searchString)
                .Select(p => p.title)
                .ToEnumerable()
                .Take(10);

            foreach(var title in pageTitles)
            {
                searchResults.OnNext(new SearchResult { Search = searchString, Title = title });
            }
        }

        static void Write(SearchResult result)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("{0}: {1}", result.Search, result.Title);
            Console.ForegroundColor = defaultColor;
        }
    }

    public struct SearchResult
    {
        public string Search { get; set; }
        public string Title { get; set; }
    }
}
