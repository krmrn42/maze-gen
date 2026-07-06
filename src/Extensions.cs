using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PlayersWorlds.Maps {
    internal static class Extensions {
        public static void ForEach<T>(this IEnumerable<T> source,
                                      Action<T> action) {
            source.ForEach((element, _) => action(element));
        }
        public static void ForEach<T>(this IEnumerable<T> source,
                                      Action<T, int> action) {
            source.ThrowIfNull("source");
            action.ThrowIfNull("action");
            var i = 0;
            foreach (var element in source) {
                action(element, i);
                i++;
            }
        }

        public static void ThrowIfNull(this object item, string argName) {
            if (item == null) {
                throw new ArgumentNullException(argName);
            }
        }

        public static void ThrowIfNullOrEmpty(this IEnumerable item, string argName) {
            if (item == null) {
                throw new ArgumentNullException(argName);
            }
            if (!item.GetEnumerator().MoveNext()) {
                throw new ArgumentException(argName + " is empty.");
            }
        }

        public static BaseStats Stats(this IEnumerable<int> values) =>
            BaseStats.From(values.Select(x => (double)x));

        public static string DebugString(this object item) {
            return item.DebugString((member, value) => $"\t{member.Name} = {value}\n");
        }

        public static string DebugString(this object item, Func<MemberInfo, string, string> memberFormatter) {
            var members = item.GetType().GetMembers(
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance);
            var values = new List<string>();
            foreach (var member in members) {
                object value;
                Type type;
                if (member is System.Reflection.FieldInfo field) {
                    value = field.GetValue(item);
                    type = field.FieldType;
                } else if (member is System.Reflection.PropertyInfo property) {
                    value = property.GetValue(item);
                    type = property.PropertyType;
                } else continue;
                string strValue;
                if (value == null) {
                    strValue = "<null>";
                } else if (value is ICollection collection) {
                    var generic = string.Join(",", type.GetGenericArguments().Select(a => a.Name));
                    strValue = type.Name +
                        (generic.Length > 0 ? $"<{generic}>" : "") +
                        "(" + string.Join(", ", collection.Cast<object>().Take(5)) +
                        (collection.Count > 5 ? $"...({collection.Count})" : "") + ")";
                } else {
                    strValue = value.ToString();
                }
                values.Add(memberFormatter(member, strValue));
            }
            var valuesStr = "(" + string.Join(", ", values.Take(50)) + (values.Count > 50 ? $"...({values.Count})" : "") + ")";
            return item.GetType().FullName + valuesStr;
        }

        public static void Set<K, V>(this Dictionary<K, V> dictionary,
            K key, V value) {
            if (dictionary.ContainsKey(key)) {
                dictionary[key] = value;
            } else {
                dictionary.Add(key, value);
            }
        }

        public static void Set<K, V>(this Dictionary<K, List<V>> dictionary,
            K key, V value) {
            if (dictionary.ContainsKey(key)) {
                dictionary[key].Add(value);
            } else {
                dictionary.Add(key, new List<V>() { value });
            }
        }

        public static IEnumerable<(K, V)> GetAll<K, V>(
            this Dictionary<K, V> dictionary,
            IEnumerable<K> keys) {
            foreach (var k in keys) {
                if (dictionary.ContainsKey(k)) {
                    yield return (k, dictionary[k]);
                }
            }
        }

        public static IEnumerable<K> GetAll<K>(this ICollection<K> collection,
            IEnumerable<K> keys) {
            foreach (var k in keys) {
                if (collection.Contains(k)) {
                    yield return k;
                }
            }
        }

        public static bool TryDequeue<T>(this Queue<T> queue, out T result) {
            try {
                result = queue.Dequeue();
                return true;
            } catch (InvalidOperationException) {
                result = default;
                return false;
            }
        }

        public static bool IsZero(this double value) => Math.Abs(value) < VectorD.MIN;
    }

    internal class BaseStats {
        public double Min { get; private set; }
        public double Max { get; private set; }
        public double Mean { get; private set; }
        public double Median { get; private set; }
        public double Mode { get; private set; }
        public double Stddev { get; private set; }
        public double Variance { get; private set; }
        public int Count { get; private set; }

        public static BaseStats From(IEnumerable<double> values) {
            values.ThrowIfNull("values");
            var list = values.ToList();
            var stats = new BaseStats {
                Count = list.Count
            };
            if (stats.Count > 0) {
                stats.Min = list.Min();
                stats.Max = list.Max();
                stats.Mean = list.Average();
                stats.Median = list[list.Count / 2];
                stats.Mode = list
                    .GroupBy(v => v)
                    .OrderByDescending(g => g.Count())
                    .First().Key;
                stats.Variance = list.Select(v => v * v).Average() - stats.Mean * stats.Mean;
                stats.Stddev = Math.Sqrt(stats.Variance);
            }
            return stats;
        }
    }
}
