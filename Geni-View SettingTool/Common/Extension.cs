using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Geni_View_SettingTool.Common
{
    //Collect all the innerException
    [ExcludeFromCodeCoverage]
    public static class ExceptionHelp
    {
        public static IEnumerable<TSource> FromHierarchy<TSource>(
            this TSource source,
            Func<TSource, TSource> nextItem,
            Func<TSource, bool> canContinue)
        {
            for (var current = source; canContinue(current); current = nextItem(current))
            {
                yield return current;
            }
        }

        public static IEnumerable<TSource> FromHierarchy<TSource>(
            this TSource source,
            Func<TSource, TSource> nextItem)
            where TSource : class
        {
            return FromHierarchy(source, nextItem, s => s != null);
        }

        /// <summary>
        /// Collect  innerException
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string CollectInnerException(this Exception ex)
        {
            //Collect  innerException
            var messages = ex.FromHierarchy(x => x.InnerException).Select(x => x.Message).ToList();
            messages.Add(DateTime.Now.ToString(Global._dateTimeFormat));
            var message = String.Join(". ", messages);
            return message;
        }
    }
}