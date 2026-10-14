namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy span windows by code.</summary>
public interface ILegacySpanWindowStore
{
    LegacySpanWindow? Find(string code);
    void Save(LegacySpanWindow item);
    bool Remove(string code);
    List<LegacySpanWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacySpanWindowStore(Dictionary<string, LegacySpanWindow> items, List<string> order) : ILegacySpanWindowStore
{
    public Dictionary<string, LegacySpanWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacySpanWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacySpanWindow item)
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

    public List<LegacySpanWindow> List()
    {
        var outValue = new List<LegacySpanWindow>();
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
public sealed class CachedLegacySpanWindowStore(MemoryLegacySpanWindowStore inner, LegacySpanWindowCache cache, int hits, int misses) : ILegacySpanWindowStore
{
    public MemoryLegacySpanWindowStore Inner { get; set; } = inner;
    public LegacySpanWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacySpanWindow? Find(string code)
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

    public void Save(LegacySpanWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacySpanWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy span windows. Empty fields match all.</summary>
public sealed record LegacySpanWindowQuery(string Label, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(LegacySpanWindow item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacySpanWindow> Apply(List<LegacySpanWindow> items)
    {
        var matched = Collections.Filter(items, (LegacySpanWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy span window store.</summary>
public static class LegacySpanWindowStore
{
    public static ILegacySpanWindowStore NewMemoryLegacySpanWindowStore()
    {
        return new MemoryLegacySpanWindowStore(new Dictionary<string, LegacySpanWindow>(), new List<string>());
    }

    public static ILegacySpanWindowStore NewCachedLegacySpanWindowStore(MemoryLegacySpanWindowStore inner, int ttl, IClock clock)
    {
        return new CachedLegacySpanWindowStore(inner, LegacySpanWindowCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LegacySpanWindow> FindLegacySpanWindows(ILegacySpanWindowStore store, LegacySpanWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
