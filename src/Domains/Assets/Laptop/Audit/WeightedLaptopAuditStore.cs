namespace Acme.Workforce.Assets.Laptop.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted laptop audits by code.</summary>
public interface IWeightedLaptopAuditStore
{
    WeightedLaptopAudit? Find(string code);
    void Save(WeightedLaptopAudit item);
    bool Remove(string code);
    List<WeightedLaptopAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedLaptopAuditStore(Dictionary<string, WeightedLaptopAudit> items, List<string> order) : IWeightedLaptopAuditStore
{
    public Dictionary<string, WeightedLaptopAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedLaptopAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedLaptopAudit item)
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

    public List<WeightedLaptopAudit> List()
    {
        var outValue = new List<WeightedLaptopAudit>();
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
public sealed class CachedWeightedLaptopAuditStore(MemoryWeightedLaptopAuditStore inner, WeightedLaptopAuditCache cache, int hits, int misses) : IWeightedLaptopAuditStore
{
    public MemoryWeightedLaptopAuditStore Inner { get; set; } = inner;
    public WeightedLaptopAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedLaptopAudit? Find(string code)
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

    public void Save(WeightedLaptopAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedLaptopAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted laptop audits. Empty fields match all.</summary>
public sealed record WeightedLaptopAuditQuery(string Region, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedLaptopAudit item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedLaptopAudit> Apply(List<WeightedLaptopAudit> items)
    {
        var matched = Collections.Filter(items, (WeightedLaptopAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted laptop audit store.</summary>
public static class WeightedLaptopAuditStore
{
    public static IWeightedLaptopAuditStore NewMemoryWeightedLaptopAuditStore()
    {
        return new MemoryWeightedLaptopAuditStore(new Dictionary<string, WeightedLaptopAudit>(), new List<string>());
    }

    public static IWeightedLaptopAuditStore NewCachedWeightedLaptopAuditStore(MemoryWeightedLaptopAuditStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedLaptopAuditStore(inner, WeightedLaptopAuditCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<WeightedLaptopAudit> FindWeightedLaptopAudits(IWeightedLaptopAuditStore store, WeightedLaptopAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
