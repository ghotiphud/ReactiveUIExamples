using LinqToWiki;
using LinqToWiki.Generated;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Concurrency;
using System.Diagnostics;

namespace ReactiveWiki
{
    public class WikiVM : ReactiveObject
    {
        static Wiki wiki = new Wiki("LinqToWiki UserAgent Test", "en.wikipedia.org"); //https://en.wikiquote.org

        string _searchText = String.Empty;
        public string SearchText
        {
            get { return _searchText; }
            set { this.RaiseAndSetIfChanged(ref _searchText, value); }
        }

        public ReactiveList<string> AutoComplete { get; set; }

        public ReactiveCommand<string> Navigate { get; set; }

        ObservableAsPropertyHelper<string> _url;
        public string URL { get { return _url.Value; } }

        public WikiVM()
        {
            AutoComplete = new ReactiveList<string>();

            this.WhenAnyValue(t => t.SearchText)
                .ObserveOn(Scheduler.Default)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Where(search => search.Length > 3 && !AutoComplete.Contains(search))
                .Select(search => GetWikiTitles(search))
                .ObserveOnDispatcher()
                .Subscribe(results => PopulateAutoComplete(results));

            Navigate = ReactiveCommand.CreateAsyncTask(x => GetWikiURL(SearchText));
            Navigate.ToProperty(this, t => t.URL, out _url, "http://en.wikipedia.org");
        }

        private Task<string> GetWikiURL(string searchText)
        {
            var task = Task.Factory.StartNew(() =>
            {
                var pages = wiki.Query.allpages()
                    .Where(p => p.filterredir == allpagesfilterredir.nonredirects)
                    .Where(p => p.prefix == searchText)
                    .Pages;

                var pageUrl = pages.Select(p => p.info.fullurl).ToEnumerable().First();

                return pageUrl;
            });

            return task;
        }

        private IEnumerable<string> GetWikiTitles(string searchText)
        {
            var pageTitles = wiki.Query.search(searchText)
                .Where(p => p.redirects == false)
                .Select(p => p.title)
                .ToEnumerable()
                .Take(10);

            return pageTitles;
        }

        private void PopulateAutoComplete(IEnumerable<string> searchResults)
        {
            AutoComplete.Clear();
            AutoComplete.AddRange(searchResults);
        }
    }
}
