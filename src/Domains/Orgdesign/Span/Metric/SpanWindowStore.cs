namespace Acme.Workforce.Orgdesign.Span.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span windows by code.</summary>
public interface ISpanWindowStore
{
    SpanWindow? Find(string code);
    void Save(SpanWindow item);
    bool Remove(string code);
    List<SpanWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanWindowStore(Dictionary<string, SpanWindow> items, List<string> order) : ISpanWindowStore
{
    public Dictionary<string, SpanWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanWindow item)
    {
        if (!Items.ContainsKey(item.Code))
        {
            Order.Add(item.Code);
        }
        Items[item.Code] = item;
    }

    public bool Remove(string code)
    {
        if (!Items.ContainsKey(code))
        {
            return false;
        }
        Items.Remove(code);
        var kept = new List<string>();
        foreach (var existing in Order)
        {
            if (existing != code)
            {
                kept.Add(existing);
            }
        }
        Order = kept;
        return true;
    }

    public List<SpanWindow> List()
    {
        var outValue = new List<SpanWindow>();
        foreach (var key in Order)
        {
            if (!Items.TryGetValue(key, out var found))
            {
                continue;
            }
            outValue.Add(found);
        }
        return outValue;
    }
}

/// <summary>Serves reads from a short-lived cache in front of another store.</summary>
public sealed class CachedSpanWindowStore(MemorySpanWindowStore inner, SpanWindowCache cache, int hits, int misses) : ISpanWindowStore
{
    public MemorySpanWindowStore Inner { get; set; } = inner;
    public SpanWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanWindow? Find(string code)
    {
        if (Cache.Get(code) is not { } hit)
        {
            Misses += 1;
            if (Inner.Find(code) is not { } found)
            {
                return null;
            }
            Cache.Put(code, found);
            return found;
        }
        Hits += 1;
        return hit;
    }

    public void Save(SpanWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span windows. Empty fields match all.</summary>
public sealed record SpanWindowQuery(string Country, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(SpanWindow item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanWindow> Apply(List<SpanWindow> items)
    {
        var matched = Collections.Filter(items, (SpanWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span window store.</summary>
public static class SpanWindowStore
{
    public static ISpanWindowStore NewMemorySpanWindowStore()
    {
        return new MemorySpanWindowStore(new Dictionary<string, SpanWindow>(), new List<string>());
    }

    public static ISpanWindowStore NewCachedSpanWindowStore(MemorySpanWindowStore inner, int ttl, IClock clock)
    {
        return new CachedSpanWindowStore(inner, SpanWindowCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<SpanWindow> FindSpanWindows(ISpanWindowStore store, SpanWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
