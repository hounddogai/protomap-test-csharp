namespace Acme.Workforce.Talent.Mobility.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility audits by code.</summary>
public interface IMobilityAuditStore
{
    MobilityAudit? Find(string code);
    void Save(MobilityAudit item);
    bool Remove(string code);
    List<MobilityAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilityAuditStore(Dictionary<string, MobilityAudit> items, List<string> order) : IMobilityAuditStore
{
    public Dictionary<string, MobilityAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilityAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilityAudit item)
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

    public List<MobilityAudit> List()
    {
        var outValue = new List<MobilityAudit>();
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
public sealed class CachedMobilityAuditStore(MemoryMobilityAuditStore inner, MobilityAuditCache cache, int hits, int misses) : IMobilityAuditStore
{
    public MemoryMobilityAuditStore Inner { get; set; } = inner;
    public MobilityAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilityAudit? Find(string code)
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

    public void Save(MobilityAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilityAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility audits. Empty fields match all.</summary>
public sealed record MobilityAuditQuery(string Title, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(MobilityAudit item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MobilityAudit> Apply(List<MobilityAudit> items)
    {
        var matched = Collections.Filter(items, (MobilityAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility audit store.</summary>
public static class MobilityAuditStore
{
    public static IMobilityAuditStore NewMemoryMobilityAuditStore()
    {
        return new MemoryMobilityAuditStore(new Dictionary<string, MobilityAudit>(), new List<string>());
    }

    public static IMobilityAuditStore NewCachedMobilityAuditStore(MemoryMobilityAuditStore inner, int ttl, IClock clock)
    {
        return new CachedMobilityAuditStore(inner, MobilityAuditCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<MobilityAudit> FindMobilityAudits(IMobilityAuditStore store, MobilityAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
