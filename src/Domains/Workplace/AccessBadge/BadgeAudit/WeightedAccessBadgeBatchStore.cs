namespace Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted access badge batches by code.</summary>
public interface IWeightedAccessBadgeBatchStore
{
    WeightedAccessBadgeBatch? Find(string code);
    void Save(WeightedAccessBadgeBatch item);
    bool Remove(string code);
    List<WeightedAccessBadgeBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedAccessBadgeBatchStore(Dictionary<string, WeightedAccessBadgeBatch> items, List<string> order) : IWeightedAccessBadgeBatchStore
{
    public Dictionary<string, WeightedAccessBadgeBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedAccessBadgeBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedAccessBadgeBatch item)
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

    public List<WeightedAccessBadgeBatch> List()
    {
        var outValue = new List<WeightedAccessBadgeBatch>();
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
public sealed class CachedWeightedAccessBadgeBatchStore(MemoryWeightedAccessBadgeBatchStore inner, WeightedAccessBadgeBatchCache cache, int hits, int misses) : IWeightedAccessBadgeBatchStore
{
    public MemoryWeightedAccessBadgeBatchStore Inner { get; set; } = inner;
    public WeightedAccessBadgeBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedAccessBadgeBatch? Find(string code)
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

    public void Save(WeightedAccessBadgeBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedAccessBadgeBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted access badge batches. Empty fields match all.</summary>
public sealed record WeightedAccessBadgeBatchQuery(string Label, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedAccessBadgeBatch item)
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

    public List<WeightedAccessBadgeBatch> Apply(List<WeightedAccessBadgeBatch> items)
    {
        var matched = Collections.Filter(items, (WeightedAccessBadgeBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted access badge batch store.</summary>
public static class WeightedAccessBadgeBatchStore
{
    public static IWeightedAccessBadgeBatchStore NewMemoryWeightedAccessBadgeBatchStore()
    {
        return new MemoryWeightedAccessBadgeBatchStore(
            new Dictionary<string, WeightedAccessBadgeBatch>(),
            new List<string>());
    }

    public static IWeightedAccessBadgeBatchStore NewCachedWeightedAccessBadgeBatchStore(MemoryWeightedAccessBadgeBatchStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedAccessBadgeBatchStore(
            inner,
            WeightedAccessBadgeBatchCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<WeightedAccessBadgeBatch> FindWeightedAccessBadgeBatches(IWeightedAccessBadgeBatchStore store, WeightedAccessBadgeBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
