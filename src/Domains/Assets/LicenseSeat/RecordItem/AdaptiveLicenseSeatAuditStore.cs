namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive license seat audits by code.</summary>
public interface IAdaptiveLicenseSeatAuditStore
{
    AdaptiveLicenseSeatAudit? Find(string code);
    void Save(AdaptiveLicenseSeatAudit item);
    bool Remove(string code);
    List<AdaptiveLicenseSeatAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveLicenseSeatAuditStore(Dictionary<string, AdaptiveLicenseSeatAudit> items, List<string> order) : IAdaptiveLicenseSeatAuditStore
{
    public Dictionary<string, AdaptiveLicenseSeatAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveLicenseSeatAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveLicenseSeatAudit item)
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

    public List<AdaptiveLicenseSeatAudit> List()
    {
        var outValue = new List<AdaptiveLicenseSeatAudit>();
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
public sealed class CachedAdaptiveLicenseSeatAuditStore(MemoryAdaptiveLicenseSeatAuditStore inner, AdaptiveLicenseSeatAuditCache cache, int hits, int misses) : IAdaptiveLicenseSeatAuditStore
{
    public MemoryAdaptiveLicenseSeatAuditStore Inner { get; set; } = inner;
    public AdaptiveLicenseSeatAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveLicenseSeatAudit? Find(string code)
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

    public void Save(AdaptiveLicenseSeatAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveLicenseSeatAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive license seat audits. Empty fields match all.</summary>
public sealed record AdaptiveLicenseSeatAuditQuery(string Segment, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveLicenseSeatAudit item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveLicenseSeatAudit> Apply(List<AdaptiveLicenseSeatAudit> items)
    {
        var matched = Collections.Filter(items, (AdaptiveLicenseSeatAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive license seat audit store.</summary>
public static class AdaptiveLicenseSeatAuditStore
{
    public static IAdaptiveLicenseSeatAuditStore NewMemoryAdaptiveLicenseSeatAuditStore()
    {
        return new MemoryAdaptiveLicenseSeatAuditStore(
            new Dictionary<string, AdaptiveLicenseSeatAudit>(),
            new List<string>());
    }

    public static IAdaptiveLicenseSeatAuditStore NewCachedAdaptiveLicenseSeatAuditStore(MemoryAdaptiveLicenseSeatAuditStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveLicenseSeatAuditStore(
            inner,
            AdaptiveLicenseSeatAuditCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveLicenseSeatAudit> FindAdaptiveLicenseSeatAudits(IAdaptiveLicenseSeatAuditStore store, AdaptiveLicenseSeatAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
