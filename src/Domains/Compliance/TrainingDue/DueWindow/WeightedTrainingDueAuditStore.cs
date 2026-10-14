namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted training due audits by code.</summary>
public interface IWeightedTrainingDueAuditStore
{
    WeightedTrainingDueAudit? Find(string code);
    void Save(WeightedTrainingDueAudit item);
    bool Remove(string code);
    List<WeightedTrainingDueAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedTrainingDueAuditStore(Dictionary<string, WeightedTrainingDueAudit> items, List<string> order) : IWeightedTrainingDueAuditStore
{
    public Dictionary<string, WeightedTrainingDueAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedTrainingDueAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedTrainingDueAudit item)
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

    public List<WeightedTrainingDueAudit> List()
    {
        var outValue = new List<WeightedTrainingDueAudit>();
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
public sealed class CachedWeightedTrainingDueAuditStore(MemoryWeightedTrainingDueAuditStore inner, WeightedTrainingDueAuditCache cache, int hits, int misses) : IWeightedTrainingDueAuditStore
{
    public MemoryWeightedTrainingDueAuditStore Inner { get; set; } = inner;
    public WeightedTrainingDueAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedTrainingDueAudit? Find(string code)
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

    public void Save(WeightedTrainingDueAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedTrainingDueAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted training due audits. Empty fields match all.</summary>
public sealed record WeightedTrainingDueAuditQuery(string Segment, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedTrainingDueAudit item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<WeightedTrainingDueAudit> Apply(List<WeightedTrainingDueAudit> items)
    {
        var matched = Collections.Filter(items, (WeightedTrainingDueAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted training due audit store.</summary>
public static class WeightedTrainingDueAuditStore
{
    public static IWeightedTrainingDueAuditStore NewMemoryWeightedTrainingDueAuditStore()
    {
        return new MemoryWeightedTrainingDueAuditStore(
            new Dictionary<string, WeightedTrainingDueAudit>(),
            new List<string>());
    }

    public static IWeightedTrainingDueAuditStore NewCachedWeightedTrainingDueAuditStore(MemoryWeightedTrainingDueAuditStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedTrainingDueAuditStore(
            inner,
            WeightedTrainingDueAuditCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<WeightedTrainingDueAudit> FindWeightedTrainingDueAudits(IWeightedTrainingDueAuditStore store, WeightedTrainingDueAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
