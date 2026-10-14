namespace Acme.Workforce.Orgdesign.Span.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span baselines by code.</summary>
public interface ISpanBaselineStore
{
    SpanBaseline? Find(string code);
    void Save(SpanBaseline item);
    bool Remove(string code);
    List<SpanBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanBaselineStore(Dictionary<string, SpanBaseline> items, List<string> order) : ISpanBaselineStore
{
    public Dictionary<string, SpanBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanBaseline item)
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

    public List<SpanBaseline> List()
    {
        var outValue = new List<SpanBaseline>();
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
public sealed class CachedSpanBaselineStore(MemorySpanBaselineStore inner, SpanBaselineCache cache, int hits, int misses) : ISpanBaselineStore
{
    public MemorySpanBaselineStore Inner { get; set; } = inner;
    public SpanBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanBaseline? Find(string code)
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

    public void Save(SpanBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span baselines. Empty fields match all.</summary>
public sealed record SpanBaselineQuery(string Name, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(SpanBaseline item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanBaseline> Apply(List<SpanBaseline> items)
    {
        var matched = Collections.Filter(items, (SpanBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span baseline store.</summary>
public static class SpanBaselineStore
{
    public static ISpanBaselineStore NewMemorySpanBaselineStore()
    {
        return new MemorySpanBaselineStore(new Dictionary<string, SpanBaseline>(), new List<string>());
    }

    public static ISpanBaselineStore NewCachedSpanBaselineStore(MemorySpanBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedSpanBaselineStore(inner, SpanBaselineCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SpanBaseline> FindSpanBaselines(ISpanBaselineStore store, SpanBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
