namespace Acme.Workforce.Assets.Assignment.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted assignment audits by code.</summary>
public interface IWeightedAssignmentAuditStore
{
    WeightedAssignmentAudit? Find(string code);
    void Save(WeightedAssignmentAudit item);
    bool Remove(string code);
    List<WeightedAssignmentAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedAssignmentAuditStore(Dictionary<string, WeightedAssignmentAudit> items, List<string> order) : IWeightedAssignmentAuditStore
{
    public Dictionary<string, WeightedAssignmentAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedAssignmentAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedAssignmentAudit item)
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

    public List<WeightedAssignmentAudit> List()
    {
        var outValue = new List<WeightedAssignmentAudit>();
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
public sealed class CachedWeightedAssignmentAuditStore(MemoryWeightedAssignmentAuditStore inner, WeightedAssignmentAuditCache cache, int hits, int misses) : IWeightedAssignmentAuditStore
{
    public MemoryWeightedAssignmentAuditStore Inner { get; set; } = inner;
    public WeightedAssignmentAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedAssignmentAudit? Find(string code)
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

    public void Save(WeightedAssignmentAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedAssignmentAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted assignment audits. Empty fields match all.</summary>
public sealed record WeightedAssignmentAuditQuery(string Title, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedAssignmentAudit item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedAssignmentAudit> Apply(List<WeightedAssignmentAudit> items)
    {
        var matched = Collections.Filter(items, (WeightedAssignmentAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted assignment audit store.</summary>
public static class WeightedAssignmentAuditStore
{
    public static IWeightedAssignmentAuditStore NewMemoryWeightedAssignmentAuditStore()
    {
        return new MemoryWeightedAssignmentAuditStore(
            new Dictionary<string, WeightedAssignmentAudit>(),
            new List<string>());
    }

    public static IWeightedAssignmentAuditStore NewCachedWeightedAssignmentAuditStore(MemoryWeightedAssignmentAuditStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedAssignmentAuditStore(
            inner,
            WeightedAssignmentAuditCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<WeightedAssignmentAudit> FindWeightedAssignmentAudits(IWeightedAssignmentAuditStore store, WeightedAssignmentAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
