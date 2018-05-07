using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MangaScraper.Core.Scrapers {
  [Flags]
  public enum Genre : ulong {
    None          = 0,
    Action        = 1L << 0,
    Adventure     = 1L << 1,
    Comedy        = 1L << 2,
    Demons        = 1L << 3,
    Drama         = 1L << 4,
    Ecchi         = 1L << 5,
    Fantasy       = 1L << 6,
    GenderBender  = 1L << 7,
    Harem         = 1L << 8,
    Historical    = 1L << 9,
    Horror        = 1L << 10,
    Josei         = 1L << 11,
    Magic         = 1L << 12,
    MartialArts   = 1L << 13,
    Mature        = 1L << 14,
    Mecha         = 1L << 15,
    Military      = 1L << 16,
    Mystery       = 1L << 17,
    OneShot       = 1L << 18,
    Psychological = 1L << 19,
    Romance       = 1L << 20,
    SchoolLife    = 1L << 21,
    SciFi         = 1L << 22,
    Seinen        = 1L << 23,
    Shoujo        = 1L << 24,
    Shoujoai      = 1L << 25,
    Shounen       = 1L << 26,
    Shounenai     = 1L << 27,
    SliceOfLife   = 1L << 28,
    Smut          = 1L << 29,
    Sports        = 1L << 30,
    SuperPower    = 1L << 31,
    Supernatural  = 1L << 32,
    Tragedy       = 1L << 33,
    Vampire       = 1L << 34,
    Yaoi          = 1L << 35,
    Yuri          = 1L << 36
  }

  public static class GenreExtensions {
    private static Dictionary<string, Genre> Translations { get; }
    static GenreExtensions() => Translations = CreateDictionary();

    private static Dictionary<string, Genre> CreateDictionary() {
      var dict = new Dictionary<string, Genre>();
      foreach (var genre in Enum.GetValues(typeof(Genre)).Cast<Genre>()) {
        var enumString = genre.ToString();
        dict.Add(enumString, genre);
        var multiWordEnum = enumString.ParsePascalCase();
        if (multiWordEnum.Length > 1) {
          dict.Add(string.Join(" ", multiWordEnum), genre);
          dict.Add(string.Join("-", multiWordEnum), genre);
        }
      }
      return dict;
    }

    private static readonly Regex PascalRegex = new Regex("(^[a-z]+|[A-Z]+(?![a-z])|[A-Z][a-z]+)");

    private static string[] ParsePascalCase(this string s) =>
      PascalRegex.Matches(s)
      .OfType<Match>()
      .Select(m => m.Value)
      .ToArray();

    public static Genre ParseAsGenre(this string genre) => Translations.ContainsKey(genre) ? Translations[genre] : Genre.None;

    public static Genre Merge(this IEnumerable<Genre> genres) => genres.Aggregate((a, c) => a | c);

    public static IEnumerable<Genre> Split(this Genre genre) =>
      Enum.GetValues(typeof(Genre))
      .Cast<Genre>()
      .Where(e => e != Genre.None)
      .Where(a => genre.HasFlag(a));
  }
}