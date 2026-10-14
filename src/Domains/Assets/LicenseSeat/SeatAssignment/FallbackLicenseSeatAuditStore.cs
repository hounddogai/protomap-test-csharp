namespace Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback license seat audits by code.</summary>
public interface IFallbackLicenseSeatAuditStore
{
    FallbackLicenseSeatAudit? Find(string code);
    void Save(FallbackLicenseSeatAudit item);
    bool Remove(string code);
    List<FallbackLicenseSeatAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackLicenseSeatAuditStore(Dictionary<string, FallbackLicenseSeatAudit> items, List<string> order) : IFallbackLicenseSeatAuditStore
{
    public Dictionary<string, FallbackLicenseSeatAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackLicenseSeatAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackLicenseSeatAudit item)
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

    public List<FallbackLicenseSeatAudit> List()
    {
        var outValue = new List<FallbackLicenseSeatAudit>();
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
public sealed class CachedFallbackLicenseSeatAuditStore(MemoryFallbackLicenseSeatAuditStore inner, FallbackLicenseSeatAuditCache cache, int hits, int misses) : IFallbackLicenseSeatAuditStore
{
    public MemoryFallbackLicenseSeatAuditStore Inner { get; set; } = inner;
    public FallbackLicenseSeatAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackLicenseSeatAudit? Find(string code)
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

    public void Save(FallbackLicenseSeatAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackLicenseSeatAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback license seat audits. Empty fields match all.</summary>
public sealed record FallbackLicenseSeatAuditQuery(string Country, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackLicenseSeatAudit item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackLicenseSeatAudit> Apply(List<FallbackLicenseSeatAudit> items)
    {
        var matched = Collections.Filter(items, (FallbackLicenseSeatAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback license seat audit store.</summary>
public static class FallbackLicenseSeatAuditStore
{
    public static IFallbackLicenseSeatAuditStore NewMemoryFallbackLicenseSeatAuditStore()
    {
        return new MemoryFallbackLicenseSeatAuditStore(
            new Dictionary<string, FallbackLicenseSeatAudit>(),
            new List<string>());
    }

    public static IFallbackLicenseSeatAuditStore NewCachedFallbackLicenseSeatAuditStore(MemoryFallbackLicenseSeatAuditStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackLicenseSeatAuditStore(
            inner,
            FallbackLicenseSeatAuditCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<FallbackLicenseSeatAudit> FindFallbackLicenseSeatAudits(IFallbackLicenseSeatAuditStore store, FallbackLicenseSeatAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
