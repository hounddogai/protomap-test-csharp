namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default training due audits by code.</summary>
public interface IDefaultTrainingDueAuditStore
{
    DefaultTrainingDueAudit? Find(string code);
    void Save(DefaultTrainingDueAudit item);
    bool Remove(string code);
    List<DefaultTrainingDueAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultTrainingDueAuditStore(Dictionary<string, DefaultTrainingDueAudit> items, List<string> order) : IDefaultTrainingDueAuditStore
{
    public Dictionary<string, DefaultTrainingDueAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultTrainingDueAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultTrainingDueAudit item)
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

    public List<DefaultTrainingDueAudit> List()
    {
        var outValue = new List<DefaultTrainingDueAudit>();
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
public sealed class CachedDefaultTrainingDueAuditStore(MemoryDefaultTrainingDueAuditStore inner, DefaultTrainingDueAuditCache cache, int hits, int misses) : IDefaultTrainingDueAuditStore
{
    public MemoryDefaultTrainingDueAuditStore Inner { get; set; } = inner;
    public DefaultTrainingDueAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultTrainingDueAudit? Find(string code)
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

    public void Save(DefaultTrainingDueAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultTrainingDueAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default training due audits. Empty fields match all.</summary>
public sealed record DefaultTrainingDueAuditQuery(string Unit, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultTrainingDueAudit item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<DefaultTrainingDueAudit> Apply(List<DefaultTrainingDueAudit> items)
    {
        var matched = Collections.Filter(items, (DefaultTrainingDueAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default training due audit store.</summary>
public static class DefaultTrainingDueAuditStore
{
    public static IDefaultTrainingDueAuditStore NewMemoryDefaultTrainingDueAuditStore()
    {
        return new MemoryDefaultTrainingDueAuditStore(
            new Dictionary<string, DefaultTrainingDueAudit>(),
            new List<string>());
    }

    public static IDefaultTrainingDueAuditStore NewCachedDefaultTrainingDueAuditStore(MemoryDefaultTrainingDueAuditStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultTrainingDueAuditStore(
            inner,
            DefaultTrainingDueAuditCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<DefaultTrainingDueAudit> FindDefaultTrainingDueAudits(IDefaultTrainingDueAuditStore store, DefaultTrainingDueAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
