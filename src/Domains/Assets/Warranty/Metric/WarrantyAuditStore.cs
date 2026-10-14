namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty audits by code.</summary>
public interface IWarrantyAuditStore
{
    WarrantyAudit? Find(string code);
    void Save(WarrantyAudit item);
    bool Remove(string code);
    List<WarrantyAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyAuditStore(Dictionary<string, WarrantyAudit> items, List<string> order) : IWarrantyAuditStore
{
    public Dictionary<string, WarrantyAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyAudit item)
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

    public List<WarrantyAudit> List()
    {
        var outValue = new List<WarrantyAudit>();
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
public sealed class CachedWarrantyAuditStore(MemoryWarrantyAuditStore inner, WarrantyAuditCache cache, int hits, int misses) : IWarrantyAuditStore
{
    public MemoryWarrantyAuditStore Inner { get; set; } = inner;
    public WarrantyAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyAudit? Find(string code)
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

    public void Save(WarrantyAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty audits. Empty fields match all.</summary>
public sealed record WarrantyAuditQuery(string Unit, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyAudit item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyAudit> Apply(List<WarrantyAudit> items)
    {
        var matched = Collections.Filter(items, (WarrantyAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty audit store.</summary>
public static class WarrantyAuditStore
{
    public static IWarrantyAuditStore NewMemoryWarrantyAuditStore()
    {
        return new MemoryWarrantyAuditStore(new Dictionary<string, WarrantyAudit>(), new List<string>());
    }

    public static IWarrantyAuditStore NewCachedWarrantyAuditStore(MemoryWarrantyAuditStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyAuditStore(inner, WarrantyAuditCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<WarrantyAudit> FindWarrantyAudits(IWarrantyAuditStore store, WarrantyAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
