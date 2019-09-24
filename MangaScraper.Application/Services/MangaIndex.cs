using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MangaScraper.Application.Persistence;
using MangaScraper.Core.Scrapers;
using MangaScraper.Core.Scrapers.Manga;

namespace MangaScraper.Application.Services
{
    public class MangaIndex : IMangaIndex
    {
        private readonly IMangaDownloader _manager;
        private readonly IMetaDataRepository _metaDataService;
        private readonly IMemCache _memCache;

        private Task<MangaInfo[]> MyDictionary { get; set; }


        public MangaIndex(IMangaDownloader manager, IMetaDataRepository metaDataService, IMemCache memCache)
        {
            _manager = manager;
            _metaDataService = metaDataService;
            _memCache = memCache;
            MyDictionary = CreateDictionary();
            //todo
        }

        private async Task<MangaInfo[]> CreateDictionary()
        {
            var mangaData = await _memCache.GetAsync();
            var metaData = await _metaDataService.GetMetaData();
            var metaDataDict = metaData.Where(m => !string.IsNullOrEmpty(m.name))
                .GroupBy(m => m.name)
                .ToDictionary(m => m.Key, x => x.First().metaData);


            var dict = mangaData //
                .GroupBy(t => t.name, t => (t.provider, t.url))
                .Select(g => new MangaInfo
                {
                    Name = g.Key,
                    Instances = g.ToList(),
                    MetaData = metaDataDict.ContainsKey(g.Key) ? metaDataDict[g.Key] : default
                })
                .ToArray();
            return dict;
        }


        public async Task<IEnumerable<MangaInfo>> FindMangas(Genre genres)
        {
            //todo index, store to disk, etc
            var dict = await MyDictionary;
            return dict
                .AsParallel()
                .Where(kvp => kvp.MetaData.Genres.HasFlag(genres))
                .ToList();
        }

        public async Task<IEnumerable<MangaInfo>> FindMangas(string name)
        {
            //todo index, store to disk, etc
            var dict = await MyDictionary;
            return dict
                .AsParallel()
                .Where(kvp => kvp.Name.ToLowerInvariant().Contains(name.ToLowerInvariant()))
                .ToList();
        }

        public async Task<IEnumerable<MangaInfo>> FindMangasByArtist(string artist)
        {
            //todo index, store to disk, etc
            var dict = await MyDictionary;
            return dict
                .AsParallel()
                .GroupBy(kvp => kvp.MetaData.Artist)
                .Where(kvp => kvp.Key?.ToUpperInvariant().Contains(artist.ToUpperInvariant()) == true)
                .SelectMany(kvp => kvp)
                .ToList();
        }

        public IReadOnlyCollection<string> Providers => _manager.Providers.ToList();

        public Task Update() => Update(null);

        public async Task Update(GetProgress factory)
        {
            var result = new List<(IEnumerable<(string name, string url)> data, string provider)>(_manager.Providers.Count());
            foreach (var provider in _manager.Providers)
            {
                var data = await _manager.ListInstances(provider, factory?.Invoke(provider));
                result.Add((data, provider));
            }

            var group = result.AsParallel().SelectMany(q => q.data, (x, t) => (x.provider, t.name, t.url));
            await _memCache.WriteToDisk(group);

            MyDictionary = CreateDictionary();


            //todo create index, store to disk, etc
        }

        public async Task<string> GetCoverUrl(string provider, string url)
        {
            try
            {
                return await _manager.CoverUrl(provider, url);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public Task<IEnumerable<IChapterParser>> Chapters(string provider, string url)
        {
            return _manager.ChapterParsers(provider, url);
        }

        public Task DownloadChapter(IChapterParser parser, string basePath, IProgress<double> progress = null) =>
            _manager.DownloadChapterTo(parser, basePath, progress);
    }
}