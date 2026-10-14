namespace Acme.Workforce.Orgdesign.Span.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span groups by code.</summary>
public interface ISpanGroupStore
{
    SpanGroup? Find(string code);
    void Save(SpanGroup item);
    bool Remove(string code);
    List<SpanGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanGroupStore(Dictionary<string, SpanGroup> items, List<string> order) : ISpanGroupStore
{
    public Dictionary<string, SpanGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanGroup item)
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

    public List<SpanGroup> List()
    {
        var outValue = new List<SpanGroup>();
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
public sealed class CachedSpanGroupStore(MemorySpanGroupStore inner, SpanGroupCache cache, int hits, int misses) : ISpanGroupStore
{
    public MemorySpanGroupStore Inner { get; set; } = inner;
    public SpanGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanGroup? Find(string code)
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

    public void Save(SpanGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span groups. Empty fields match all.</summary>
public sealed record SpanGroupQuery(string Department, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(SpanGroup item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<SpanGroup> Apply(List<SpanGroup> items)
    {
        var matched = Collections.Filter(items, (SpanGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span group store.</summary>
public static class SpanGroupStore
{
    public static ISpanGroupStore NewMemorySpanGroupStore()
    {
        return new MemorySpanGroupStore(new Dictionary<string, SpanGroup>(), new List<string>());
    }

    public static ISpanGroupStore NewCachedSpanGroupStore(MemorySpanGroupStore inner, int ttl, IClock clock)
    {
        return new CachedSpanGroupStore(inner, SpanGroupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SpanGroup> FindSpanGroups(ISpanGroupStore store, SpanGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
