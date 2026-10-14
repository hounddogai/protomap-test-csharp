namespace Acme.Workforce.Assets.Laptop.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop audits by code.</summary>
public interface ILaptopAuditStore
{
    LaptopAudit? Find(string code);
    void Save(LaptopAudit item);
    bool Remove(string code);
    List<LaptopAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopAuditStore(Dictionary<string, LaptopAudit> items, List<string> order) : ILaptopAuditStore
{
    public Dictionary<string, LaptopAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopAudit item)
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

    public List<LaptopAudit> List()
    {
        var outValue = new List<LaptopAudit>();
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
public sealed class CachedLaptopAuditStore(MemoryLaptopAuditStore inner, LaptopAuditCache cache, int hits, int misses) : ILaptopAuditStore
{
    public MemoryLaptopAuditStore Inner { get; set; } = inner;
    public LaptopAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopAudit? Find(string code)
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

    public void Save(LaptopAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop audits. Empty fields match all.</summary>
public sealed record LaptopAuditQuery(string Name, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopAudit item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopAudit> Apply(List<LaptopAudit> items)
    {
        var matched = Collections.Filter(items, (LaptopAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop audit store.</summary>
public static class LaptopAuditStore
{
    public static ILaptopAuditStore NewMemoryLaptopAuditStore()
    {
        return new MemoryLaptopAuditStore(new Dictionary<string, LaptopAudit>(), new List<string>());
    }

    public static ILaptopAuditStore NewCachedLaptopAuditStore(MemoryLaptopAuditStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopAuditStore(inner, LaptopAuditCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LaptopAudit> FindLaptopAudits(ILaptopAuditStore store, LaptopAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
