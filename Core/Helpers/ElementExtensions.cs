using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom;

namespace MangaScraper.Core.Helpers {
  public static class ElementExtensions {
    public static IElement Element(this IElement e, string typeName) {
      if (e == null)
        return null;
      return e.GetElementsByTagName(typeName).FirstOrDefault();
    }

    public static IElement GetNextSiblingWithText(this IElement e, string text) {
      var next = e?.NextElementSibling;
      while (next != null && next.TextContent != text) {
        next = next.NextElementSibling;
      }
      return next;
    }

    public static IEnumerable<IElement> Elements(this IElement e, string typeName) => e.GetElementsByTagName(typeName);

    public static bool HasClass(this IElement e, string typeName) => e.ClassList.Contains(typeName);

    public static IElement GetFirstChildByType(this IElement e, string typeName) => e?.GetFirstChild(c => c.LocalName == typeName);

    public static IElement GetFirstChild(this IElement e, Predicate<IElement> p) => e?.Children.FirstOrDefault(c => p(c));

  }
}