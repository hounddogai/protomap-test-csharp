namespace Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict scenario quotas by code.</summary>
public interface IStrictScenarioQuotaStore
{
    StrictScenarioQuota? Find(string code);
    void Save(StrictScenarioQuota item);
    bool Remove(string code);
    List<StrictScenarioQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictScenarioQuotaStore(Dictionary<string, StrictScenarioQuota> items, List<string> order) : IStrictScenarioQuotaStore
{
    public Dictionary<string, StrictScenarioQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictScenarioQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictScenarioQuota item)
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

    public List<StrictScenarioQuota> List()
    {
        var outValue = new List<StrictScenarioQuota>();
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
public sealed class CachedStrictScenarioQuotaStore(MemoryStrictScenarioQuotaStore inner, StrictScenarioQuotaCache cache, int hits, int misses) : IStrictScenarioQuotaStore
{
    public MemoryStrictScenarioQuotaStore Inner { get; set; } = inner;
    public StrictScenarioQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictScenarioQuota? Find(string code)
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

    public void Save(StrictScenarioQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictScenarioQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict scenario quotas. Empty fields match all.</summary>
public sealed record StrictScenarioQuotaQuery(string Country, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(StrictScenarioQuota item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictScenarioQuota> Apply(List<StrictScenarioQuota> items)
    {
        var matched = Collections.Filter(items, (StrictScenarioQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict scenario quota store.</summary>
public static class StrictScenarioQuotaStore
{
    public static IStrictScenarioQuotaStore NewMemoryStrictScenarioQuotaStore()
    {
        return new MemoryStrictScenarioQuotaStore(new Dictionary<string, StrictScenarioQuota>(), new List<string>());
    }

    public static IStrictScenarioQuotaStore NewCachedStrictScenarioQuotaStore(MemoryStrictScenarioQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedStrictScenarioQuotaStore(inner, StrictScenarioQuotaCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<StrictScenarioQuota> FindStrictScenarioQuotas(IStrictScenarioQuotaStore store, StrictScenarioQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
