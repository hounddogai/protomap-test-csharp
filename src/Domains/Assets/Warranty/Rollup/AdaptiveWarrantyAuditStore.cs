namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive warranty audits by code.</summary>
public interface IAdaptiveWarrantyAuditStore
{
    AdaptiveWarrantyAudit? Find(string code);
    void Save(AdaptiveWarrantyAudit item);
    bool Remove(string code);
    List<AdaptiveWarrantyAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveWarrantyAuditStore(Dictionary<string, AdaptiveWarrantyAudit> items, List<string> order) : IAdaptiveWarrantyAuditStore
{
    public Dictionary<string, AdaptiveWarrantyAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveWarrantyAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveWarrantyAudit item)
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

    public List<AdaptiveWarrantyAudit> List()
    {
        var outValue = new List<AdaptiveWarrantyAudit>();
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
public sealed class CachedAdaptiveWarrantyAuditStore(MemoryAdaptiveWarrantyAuditStore inner, AdaptiveWarrantyAuditCache cache, int hits, int misses) : IAdaptiveWarrantyAuditStore
{
    public MemoryAdaptiveWarrantyAuditStore Inner { get; set; } = inner;
    public AdaptiveWarrantyAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveWarrantyAudit? Find(string code)
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

    public void Save(AdaptiveWarrantyAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveWarrantyAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive warranty audits. Empty fields match all.</summary>
public sealed record AdaptiveWarrantyAuditQuery(string Locale, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveWarrantyAudit item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveWarrantyAudit> Apply(List<AdaptiveWarrantyAudit> items)
    {
        var matched = Collections.Filter(items, (AdaptiveWarrantyAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive warranty audit store.</summary>
public static class AdaptiveWarrantyAuditStore
{
    public static IAdaptiveWarrantyAuditStore NewMemoryAdaptiveWarrantyAuditStore()
    {
        return new MemoryAdaptiveWarrantyAuditStore(
            new Dictionary<string, AdaptiveWarrantyAudit>(),
            new List<string>());
    }

    public static IAdaptiveWarrantyAuditStore NewCachedAdaptiveWarrantyAuditStore(MemoryAdaptiveWarrantyAuditStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveWarrantyAuditStore(inner, AdaptiveWarrantyAuditCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AdaptiveWarrantyAudit> FindAdaptiveWarrantyAudits(IAdaptiveWarrantyAuditStore store, AdaptiveWarrantyAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
