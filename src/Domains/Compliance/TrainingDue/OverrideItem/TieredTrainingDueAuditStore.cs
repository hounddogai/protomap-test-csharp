namespace Acme.Workforce.Compliance.TrainingDue.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered training due audits by code.</summary>
public interface ITieredTrainingDueAuditStore
{
    TieredTrainingDueAudit? Find(string code);
    void Save(TieredTrainingDueAudit item);
    bool Remove(string code);
    List<TieredTrainingDueAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredTrainingDueAuditStore(Dictionary<string, TieredTrainingDueAudit> items, List<string> order) : ITieredTrainingDueAuditStore
{
    public Dictionary<string, TieredTrainingDueAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredTrainingDueAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredTrainingDueAudit item)
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

    public List<TieredTrainingDueAudit> List()
    {
        var outValue = new List<TieredTrainingDueAudit>();
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
public sealed class CachedTieredTrainingDueAuditStore(MemoryTieredTrainingDueAuditStore inner, TieredTrainingDueAuditCache cache, int hits, int misses) : ITieredTrainingDueAuditStore
{
    public MemoryTieredTrainingDueAuditStore Inner { get; set; } = inner;
    public TieredTrainingDueAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredTrainingDueAudit? Find(string code)
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

    public void Save(TieredTrainingDueAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredTrainingDueAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered training due audits. Empty fields match all.</summary>
public sealed record TieredTrainingDueAuditQuery(string Channel, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(TieredTrainingDueAudit item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredTrainingDueAudit> Apply(List<TieredTrainingDueAudit> items)
    {
        var matched = Collections.Filter(items, (TieredTrainingDueAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered training due audit store.</summary>
public static class TieredTrainingDueAuditStore
{
    public static ITieredTrainingDueAuditStore NewMemoryTieredTrainingDueAuditStore()
    {
        return new MemoryTieredTrainingDueAuditStore(
            new Dictionary<string, TieredTrainingDueAudit>(),
            new List<string>());
    }

    public static ITieredTrainingDueAuditStore NewCachedTieredTrainingDueAuditStore(MemoryTieredTrainingDueAuditStore inner, int ttl, IClock clock)
    {
        return new CachedTieredTrainingDueAuditStore(
            inner,
            TieredTrainingDueAuditCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<TieredTrainingDueAudit> FindTieredTrainingDueAudits(ITieredTrainingDueAuditStore store, TieredTrainingDueAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
