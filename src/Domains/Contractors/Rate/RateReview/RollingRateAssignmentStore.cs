namespace Acme.Workforce.Contractors.Rate.RateReview;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling rate assignments by code.</summary>
public interface IRollingRateAssignmentStore
{
    RollingRateAssignment? Find(string code);
    void Save(RollingRateAssignment item);
    bool Remove(string code);
    List<RollingRateAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingRateAssignmentStore(Dictionary<string, RollingRateAssignment> items, List<string> order) : IRollingRateAssignmentStore
{
    public Dictionary<string, RollingRateAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingRateAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingRateAssignment item)
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

    public List<RollingRateAssignment> List()
    {
        var outValue = new List<RollingRateAssignment>();
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
public sealed class CachedRollingRateAssignmentStore(MemoryRollingRateAssignmentStore inner, RollingRateAssignmentCache cache, int hits, int misses) : IRollingRateAssignmentStore
{
    public MemoryRollingRateAssignmentStore Inner { get; set; } = inner;
    public RollingRateAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingRateAssignment? Find(string code)
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

    public void Save(RollingRateAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingRateAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling rate assignments. Empty fields match all.</summary>
public sealed record RollingRateAssignmentQuery(string Source, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(RollingRateAssignment item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingRateAssignment> Apply(List<RollingRateAssignment> items)
    {
        var matched = Collections.Filter(items, (RollingRateAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling rate assignment store.</summary>
public static class RollingRateAssignmentStore
{
    public static IRollingRateAssignmentStore NewMemoryRollingRateAssignmentStore()
    {
        return new MemoryRollingRateAssignmentStore(
            new Dictionary<string, RollingRateAssignment>(),
            new List<string>());
    }

    public static IRollingRateAssignmentStore NewCachedRollingRateAssignmentStore(MemoryRollingRateAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedRollingRateAssignmentStore(inner, RollingRateAssignmentCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RollingRateAssignment> FindRollingRateAssignments(IRollingRateAssignmentStore store, RollingRateAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
