namespace Acme.Workforce.Workplace.Desk.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk quotas by code.</summary>
public interface IDeskQuotaStore
{
    DeskQuota? Find(string code);
    void Save(DeskQuota item);
    bool Remove(string code);
    List<DeskQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskQuotaStore(Dictionary<string, DeskQuota> items, List<string> order) : IDeskQuotaStore
{
    public Dictionary<string, DeskQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskQuota item)
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

    public List<DeskQuota> List()
    {
        var outValue = new List<DeskQuota>();
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
public sealed class CachedDeskQuotaStore(MemoryDeskQuotaStore inner, DeskQuotaCache cache, int hits, int misses) : IDeskQuotaStore
{
    public MemoryDeskQuotaStore Inner { get; set; } = inner;
    public DeskQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskQuota? Find(string code)
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

    public void Save(DeskQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk quotas. Empty fields match all.</summary>
public sealed record DeskQuotaQuery(string Region, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(DeskQuota item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskQuota> Apply(List<DeskQuota> items)
    {
        var matched = Collections.Filter(items, (DeskQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk quota store.</summary>
public static class DeskQuotaStore
{
    public static IDeskQuotaStore NewMemoryDeskQuotaStore()
    {
        return new MemoryDeskQuotaStore(new Dictionary<string, DeskQuota>(), new List<string>());
    }

    public static IDeskQuotaStore NewCachedDeskQuotaStore(MemoryDeskQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedDeskQuotaStore(inner, DeskQuotaCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DeskQuota> FindDeskQuotas(IDeskQuotaStore store, DeskQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
