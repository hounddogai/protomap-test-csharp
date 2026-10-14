namespace Acme.Workforce.Orgdesign.Span.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy span adjustments by code.</summary>
public interface ILegacySpanAdjustmentStore
{
    LegacySpanAdjustment? Find(string code);
    void Save(LegacySpanAdjustment item);
    bool Remove(string code);
    List<LegacySpanAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacySpanAdjustmentStore(Dictionary<string, LegacySpanAdjustment> items, List<string> order) : ILegacySpanAdjustmentStore
{
    public Dictionary<string, LegacySpanAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacySpanAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacySpanAdjustment item)
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

    public List<LegacySpanAdjustment> List()
    {
        var outValue = new List<LegacySpanAdjustment>();
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
public sealed class CachedLegacySpanAdjustmentStore(MemoryLegacySpanAdjustmentStore inner, LegacySpanAdjustmentCache cache, int hits, int misses) : ILegacySpanAdjustmentStore
{
    public MemoryLegacySpanAdjustmentStore Inner { get; set; } = inner;
    public LegacySpanAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacySpanAdjustment? Find(string code)
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

    public void Save(LegacySpanAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacySpanAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy span adjustments. Empty fields match all.</summary>
public sealed record LegacySpanAdjustmentQuery(string Name, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(LegacySpanAdjustment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<LegacySpanAdjustment> Apply(List<LegacySpanAdjustment> items)
    {
        var matched = Collections.Filter(items, (LegacySpanAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy span adjustment store.</summary>
public static class LegacySpanAdjustmentStore
{
    public static ILegacySpanAdjustmentStore NewMemoryLegacySpanAdjustmentStore()
    {
        return new MemoryLegacySpanAdjustmentStore(
            new Dictionary<string, LegacySpanAdjustment>(),
            new List<string>());
    }

    public static ILegacySpanAdjustmentStore NewCachedLegacySpanAdjustmentStore(MemoryLegacySpanAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedLegacySpanAdjustmentStore(inner, LegacySpanAdjustmentCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LegacySpanAdjustment> FindLegacySpanAdjustments(ILegacySpanAdjustmentStore store, LegacySpanAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
