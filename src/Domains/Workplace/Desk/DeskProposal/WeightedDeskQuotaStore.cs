namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted desk quotas by code.</summary>
public interface IWeightedDeskQuotaStore
{
    WeightedDeskQuota? Find(string code);
    void Save(WeightedDeskQuota item);
    bool Remove(string code);
    List<WeightedDeskQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedDeskQuotaStore(Dictionary<string, WeightedDeskQuota> items, List<string> order) : IWeightedDeskQuotaStore
{
    public Dictionary<string, WeightedDeskQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedDeskQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedDeskQuota item)
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

    public List<WeightedDeskQuota> List()
    {
        var outValue = new List<WeightedDeskQuota>();
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
public sealed class CachedWeightedDeskQuotaStore(MemoryWeightedDeskQuotaStore inner, WeightedDeskQuotaCache cache, int hits, int misses) : IWeightedDeskQuotaStore
{
    public MemoryWeightedDeskQuotaStore Inner { get; set; } = inner;
    public WeightedDeskQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedDeskQuota? Find(string code)
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

    public void Save(WeightedDeskQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedDeskQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted desk quotas. Empty fields match all.</summary>
public sealed record WeightedDeskQuotaQuery(string Label, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedDeskQuota item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedDeskQuota> Apply(List<WeightedDeskQuota> items)
    {
        var matched = Collections.Filter(items, (WeightedDeskQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted desk quota store.</summary>
public static class WeightedDeskQuotaStore
{
    public static IWeightedDeskQuotaStore NewMemoryWeightedDeskQuotaStore()
    {
        return new MemoryWeightedDeskQuotaStore(new Dictionary<string, WeightedDeskQuota>(), new List<string>());
    }

    public static IWeightedDeskQuotaStore NewCachedWeightedDeskQuotaStore(MemoryWeightedDeskQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedDeskQuotaStore(inner, WeightedDeskQuotaCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<WeightedDeskQuota> FindWeightedDeskQuotas(IWeightedDeskQuotaStore store, WeightedDeskQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
