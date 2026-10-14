namespace Acme.Workforce.Orgdesign.Span.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span overrides by code.</summary>
public interface ISpanOverrideStore
{
    SpanOverride? Find(string code);
    void Save(SpanOverride item);
    bool Remove(string code);
    List<SpanOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanOverrideStore(Dictionary<string, SpanOverride> items, List<string> order) : ISpanOverrideStore
{
    public Dictionary<string, SpanOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanOverride item)
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

    public List<SpanOverride> List()
    {
        var outValue = new List<SpanOverride>();
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
public sealed class CachedSpanOverrideStore(MemorySpanOverrideStore inner, SpanOverrideCache cache, int hits, int misses) : ISpanOverrideStore
{
    public MemorySpanOverrideStore Inner { get; set; } = inner;
    public SpanOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanOverride? Find(string code)
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

    public void Save(SpanOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span overrides. Empty fields match all.</summary>
public sealed record SpanOverrideQuery(string Source, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(SpanOverride item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanOverride> Apply(List<SpanOverride> items)
    {
        var matched = Collections.Filter(items, (SpanOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span override store.</summary>
public static class SpanOverrideStore
{
    public static ISpanOverrideStore NewMemorySpanOverrideStore()
    {
        return new MemorySpanOverrideStore(new Dictionary<string, SpanOverride>(), new List<string>());
    }

    public static ISpanOverrideStore NewCachedSpanOverrideStore(MemorySpanOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedSpanOverrideStore(inner, SpanOverrideCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SpanOverride> FindSpanOverrides(ISpanOverrideStore store, SpanOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
