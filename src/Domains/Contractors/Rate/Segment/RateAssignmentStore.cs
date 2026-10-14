namespace Acme.Workforce.Contractors.Rate.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate assignments by code.</summary>
public interface IRateAssignmentStore
{
    RateAssignment? Find(string code);
    void Save(RateAssignment item);
    bool Remove(string code);
    List<RateAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateAssignmentStore(Dictionary<string, RateAssignment> items, List<string> order) : IRateAssignmentStore
{
    public Dictionary<string, RateAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateAssignment item)
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

    public List<RateAssignment> List()
    {
        var outValue = new List<RateAssignment>();
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
public sealed class CachedRateAssignmentStore(MemoryRateAssignmentStore inner, RateAssignmentCache cache, int hits, int misses) : IRateAssignmentStore
{
    public MemoryRateAssignmentStore Inner { get; set; } = inner;
    public RateAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateAssignment? Find(string code)
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

    public void Save(RateAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate assignments. Empty fields match all.</summary>
public sealed record RateAssignmentQuery(string Title, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(RateAssignment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RateAssignment> Apply(List<RateAssignment> items)
    {
        var matched = Collections.Filter(items, (RateAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate assignment store.</summary>
public static class RateAssignmentStore
{
    public static IRateAssignmentStore NewMemoryRateAssignmentStore()
    {
        return new MemoryRateAssignmentStore(new Dictionary<string, RateAssignment>(), new List<string>());
    }

    public static IRateAssignmentStore NewCachedRateAssignmentStore(MemoryRateAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedRateAssignmentStore(inner, RateAssignmentCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RateAssignment> FindRateAssignments(IRateAssignmentStore store, RateAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
