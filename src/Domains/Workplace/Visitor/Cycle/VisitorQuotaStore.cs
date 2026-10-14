namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor quotas by code.</summary>
public interface IVisitorQuotaStore
{
    VisitorQuota? Find(string code);
    void Save(VisitorQuota item);
    bool Remove(string code);
    List<VisitorQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorQuotaStore(Dictionary<string, VisitorQuota> items, List<string> order) : IVisitorQuotaStore
{
    public Dictionary<string, VisitorQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorQuota item)
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

    public List<VisitorQuota> List()
    {
        var outValue = new List<VisitorQuota>();
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
public sealed class CachedVisitorQuotaStore(MemoryVisitorQuotaStore inner, VisitorQuotaCache cache, int hits, int misses) : IVisitorQuotaStore
{
    public MemoryVisitorQuotaStore Inner { get; set; } = inner;
    public VisitorQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorQuota? Find(string code)
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

    public void Save(VisitorQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor quotas. Empty fields match all.</summary>
public sealed record VisitorQuotaQuery(string Unit, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorQuota item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorQuota> Apply(List<VisitorQuota> items)
    {
        var matched = Collections.Filter(items, (VisitorQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor quota store.</summary>
public static class VisitorQuotaStore
{
    public static IVisitorQuotaStore NewMemoryVisitorQuotaStore()
    {
        return new MemoryVisitorQuotaStore(new Dictionary<string, VisitorQuota>(), new List<string>());
    }

    public static IVisitorQuotaStore NewCachedVisitorQuotaStore(MemoryVisitorQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorQuotaStore(inner, VisitorQuotaCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<VisitorQuota> FindVisitorQuotas(IVisitorQuotaStore store, VisitorQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
