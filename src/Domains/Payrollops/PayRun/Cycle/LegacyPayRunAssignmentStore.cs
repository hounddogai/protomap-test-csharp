namespace Acme.Workforce.Payrollops.PayRun.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy pay run assignments by code.</summary>
public interface ILegacyPayRunAssignmentStore
{
    LegacyPayRunAssignment? Find(string code);
    void Save(LegacyPayRunAssignment item);
    bool Remove(string code);
    List<LegacyPayRunAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyPayRunAssignmentStore(Dictionary<string, LegacyPayRunAssignment> items, List<string> order) : ILegacyPayRunAssignmentStore
{
    public Dictionary<string, LegacyPayRunAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyPayRunAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyPayRunAssignment item)
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

    public List<LegacyPayRunAssignment> List()
    {
        var outValue = new List<LegacyPayRunAssignment>();
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
public sealed class CachedLegacyPayRunAssignmentStore(MemoryLegacyPayRunAssignmentStore inner, LegacyPayRunAssignmentCache cache, int hits, int misses) : ILegacyPayRunAssignmentStore
{
    public MemoryLegacyPayRunAssignmentStore Inner { get; set; } = inner;
    public LegacyPayRunAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyPayRunAssignment? Find(string code)
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

    public void Save(LegacyPayRunAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyPayRunAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy pay run assignments. Empty fields match all.</summary>
public sealed record LegacyPayRunAssignmentQuery(string Name, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyPayRunAssignment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyPayRunAssignment> Apply(List<LegacyPayRunAssignment> items)
    {
        var matched = Collections.Filter(items, (LegacyPayRunAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy pay run assignment store.</summary>
public static class LegacyPayRunAssignmentStore
{
    public static ILegacyPayRunAssignmentStore NewMemoryLegacyPayRunAssignmentStore()
    {
        return new MemoryLegacyPayRunAssignmentStore(
            new Dictionary<string, LegacyPayRunAssignment>(),
            new List<string>());
    }

    public static ILegacyPayRunAssignmentStore NewCachedLegacyPayRunAssignmentStore(MemoryLegacyPayRunAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyPayRunAssignmentStore(
            inner,
            LegacyPayRunAssignmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LegacyPayRunAssignment> FindLegacyPayRunAssignments(ILegacyPayRunAssignmentStore store, LegacyPayRunAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
