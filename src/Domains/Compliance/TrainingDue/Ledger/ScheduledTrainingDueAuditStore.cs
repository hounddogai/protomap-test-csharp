namespace Acme.Workforce.Compliance.TrainingDue.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled training due audits by code.</summary>
public interface IScheduledTrainingDueAuditStore
{
    ScheduledTrainingDueAudit? Find(string code);
    void Save(ScheduledTrainingDueAudit item);
    bool Remove(string code);
    List<ScheduledTrainingDueAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledTrainingDueAuditStore(Dictionary<string, ScheduledTrainingDueAudit> items, List<string> order) : IScheduledTrainingDueAuditStore
{
    public Dictionary<string, ScheduledTrainingDueAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledTrainingDueAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledTrainingDueAudit item)
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

    public List<ScheduledTrainingDueAudit> List()
    {
        var outValue = new List<ScheduledTrainingDueAudit>();
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
public sealed class CachedScheduledTrainingDueAuditStore(MemoryScheduledTrainingDueAuditStore inner, ScheduledTrainingDueAuditCache cache, int hits, int misses) : IScheduledTrainingDueAuditStore
{
    public MemoryScheduledTrainingDueAuditStore Inner { get; set; } = inner;
    public ScheduledTrainingDueAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledTrainingDueAudit? Find(string code)
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

    public void Save(ScheduledTrainingDueAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledTrainingDueAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled training due audits. Empty fields match all.</summary>
public sealed record ScheduledTrainingDueAuditQuery(string Name, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledTrainingDueAudit item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledTrainingDueAudit> Apply(List<ScheduledTrainingDueAudit> items)
    {
        var matched = Collections.Filter(items, (ScheduledTrainingDueAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled training due audit store.</summary>
public static class ScheduledTrainingDueAuditStore
{
    public static IScheduledTrainingDueAuditStore NewMemoryScheduledTrainingDueAuditStore()
    {
        return new MemoryScheduledTrainingDueAuditStore(
            new Dictionary<string, ScheduledTrainingDueAudit>(),
            new List<string>());
    }

    public static IScheduledTrainingDueAuditStore NewCachedScheduledTrainingDueAuditStore(MemoryScheduledTrainingDueAuditStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledTrainingDueAuditStore(
            inner,
            ScheduledTrainingDueAuditCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledTrainingDueAudit> FindScheduledTrainingDueAudits(IScheduledTrainingDueAuditStore store, ScheduledTrainingDueAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
