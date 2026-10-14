namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered mobility audits by code.</summary>
public interface ITieredMobilityAuditStore
{
    TieredMobilityAudit? Find(string code);
    void Save(TieredMobilityAudit item);
    bool Remove(string code);
    List<TieredMobilityAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredMobilityAuditStore(Dictionary<string, TieredMobilityAudit> items, List<string> order) : ITieredMobilityAuditStore
{
    public Dictionary<string, TieredMobilityAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredMobilityAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredMobilityAudit item)
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

    public List<TieredMobilityAudit> List()
    {
        var outValue = new List<TieredMobilityAudit>();
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
public sealed class CachedTieredMobilityAuditStore(MemoryTieredMobilityAuditStore inner, TieredMobilityAuditCache cache, int hits, int misses) : ITieredMobilityAuditStore
{
    public MemoryTieredMobilityAuditStore Inner { get; set; } = inner;
    public TieredMobilityAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredMobilityAudit? Find(string code)
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

    public void Save(TieredMobilityAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredMobilityAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered mobility audits. Empty fields match all.</summary>
public sealed record TieredMobilityAuditQuery(string Name, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(TieredMobilityAudit item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredMobilityAudit> Apply(List<TieredMobilityAudit> items)
    {
        var matched = Collections.Filter(items, (TieredMobilityAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered mobility audit store.</summary>
public static class TieredMobilityAuditStore
{
    public static ITieredMobilityAuditStore NewMemoryTieredMobilityAuditStore()
    {
        return new MemoryTieredMobilityAuditStore(new Dictionary<string, TieredMobilityAudit>(), new List<string>());
    }

    public static ITieredMobilityAuditStore NewCachedTieredMobilityAuditStore(MemoryTieredMobilityAuditStore inner, int ttl, IClock clock)
    {
        return new CachedTieredMobilityAuditStore(inner, TieredMobilityAuditCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TieredMobilityAudit> FindTieredMobilityAudits(ITieredMobilityAuditStore store, TieredMobilityAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
