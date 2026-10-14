namespace Acme.Workforce.Compliance.TrainingDue.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due audits by code.</summary>
public interface ITrainingDueAuditStore
{
    TrainingDueAudit? Find(string code);
    void Save(TrainingDueAudit item);
    bool Remove(string code);
    List<TrainingDueAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueAuditStore(Dictionary<string, TrainingDueAudit> items, List<string> order) : ITrainingDueAuditStore
{
    public Dictionary<string, TrainingDueAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueAudit item)
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

    public List<TrainingDueAudit> List()
    {
        var outValue = new List<TrainingDueAudit>();
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
public sealed class CachedTrainingDueAuditStore(MemoryTrainingDueAuditStore inner, TrainingDueAuditCache cache, int hits, int misses) : ITrainingDueAuditStore
{
    public MemoryTrainingDueAuditStore Inner { get; set; } = inner;
    public TrainingDueAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueAudit? Find(string code)
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

    public void Save(TrainingDueAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due audits. Empty fields match all.</summary>
public sealed record TrainingDueAuditQuery(string Title, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueAudit item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<TrainingDueAudit> Apply(List<TrainingDueAudit> items)
    {
        var matched = Collections.Filter(items, (TrainingDueAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due audit store.</summary>
public static class TrainingDueAuditStore
{
    public static ITrainingDueAuditStore NewMemoryTrainingDueAuditStore()
    {
        return new MemoryTrainingDueAuditStore(new Dictionary<string, TrainingDueAudit>(), new List<string>());
    }

    public static ITrainingDueAuditStore NewCachedTrainingDueAuditStore(MemoryTrainingDueAuditStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueAuditStore(inner, TrainingDueAuditCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TrainingDueAudit> FindTrainingDueAudits(ITrainingDueAuditStore store, TrainingDueAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
