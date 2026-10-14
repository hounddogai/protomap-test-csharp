namespace Acme.Workforce.Compliance.Exception.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy exception quotas by code.</summary>
public interface ILegacyExceptionQuotaStore
{
    LegacyExceptionQuota? Find(string code);
    void Save(LegacyExceptionQuota item);
    bool Remove(string code);
    List<LegacyExceptionQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyExceptionQuotaStore(Dictionary<string, LegacyExceptionQuota> items, List<string> order) : ILegacyExceptionQuotaStore
{
    public Dictionary<string, LegacyExceptionQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyExceptionQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyExceptionQuota item)
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

    public List<LegacyExceptionQuota> List()
    {
        var outValue = new List<LegacyExceptionQuota>();
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
public sealed class CachedLegacyExceptionQuotaStore(MemoryLegacyExceptionQuotaStore inner, LegacyExceptionQuotaCache cache, int hits, int misses) : ILegacyExceptionQuotaStore
{
    public MemoryLegacyExceptionQuotaStore Inner { get; set; } = inner;
    public LegacyExceptionQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyExceptionQuota? Find(string code)
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

    public void Save(LegacyExceptionQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyExceptionQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy exception quotas. Empty fields match all.</summary>
public sealed record LegacyExceptionQuotaQuery(string Team, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyExceptionQuota item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyExceptionQuota> Apply(List<LegacyExceptionQuota> items)
    {
        var matched = Collections.Filter(items, (LegacyExceptionQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy exception quota store.</summary>
public static class LegacyExceptionQuotaStore
{
    public static ILegacyExceptionQuotaStore NewMemoryLegacyExceptionQuotaStore()
    {
        return new MemoryLegacyExceptionQuotaStore(
            new Dictionary<string, LegacyExceptionQuota>(),
            new List<string>());
    }

    public static ILegacyExceptionQuotaStore NewCachedLegacyExceptionQuotaStore(MemoryLegacyExceptionQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyExceptionQuotaStore(inner, LegacyExceptionQuotaCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LegacyExceptionQuota> FindLegacyExceptionQuotas(ILegacyExceptionQuotaStore store, LegacyExceptionQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
