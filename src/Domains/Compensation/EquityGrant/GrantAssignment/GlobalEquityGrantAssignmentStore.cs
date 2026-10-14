namespace Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global equity grant assignments by code.</summary>
public interface IGlobalEquityGrantAssignmentStore
{
    GlobalEquityGrantAssignment? Find(string code);
    void Save(GlobalEquityGrantAssignment item);
    bool Remove(string code);
    List<GlobalEquityGrantAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalEquityGrantAssignmentStore(Dictionary<string, GlobalEquityGrantAssignment> items, List<string> order) : IGlobalEquityGrantAssignmentStore
{
    public Dictionary<string, GlobalEquityGrantAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalEquityGrantAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalEquityGrantAssignment item)
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

    public List<GlobalEquityGrantAssignment> List()
    {
        var outValue = new List<GlobalEquityGrantAssignment>();
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
public sealed class CachedGlobalEquityGrantAssignmentStore(MemoryGlobalEquityGrantAssignmentStore inner, GlobalEquityGrantAssignmentCache cache, int hits, int misses) : IGlobalEquityGrantAssignmentStore
{
    public MemoryGlobalEquityGrantAssignmentStore Inner { get; set; } = inner;
    public GlobalEquityGrantAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalEquityGrantAssignment? Find(string code)
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

    public void Save(GlobalEquityGrantAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalEquityGrantAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global equity grant assignments. Empty fields match all.</summary>
public sealed record GlobalEquityGrantAssignmentQuery(string Label, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalEquityGrantAssignment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalEquityGrantAssignment> Apply(List<GlobalEquityGrantAssignment> items)
    {
        var matched = Collections.Filter(items, (GlobalEquityGrantAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global equity grant assignment store.</summary>
public static class GlobalEquityGrantAssignmentStore
{
    public static IGlobalEquityGrantAssignmentStore NewMemoryGlobalEquityGrantAssignmentStore()
    {
        return new MemoryGlobalEquityGrantAssignmentStore(
            new Dictionary<string, GlobalEquityGrantAssignment>(),
            new List<string>());
    }

    public static IGlobalEquityGrantAssignmentStore NewCachedGlobalEquityGrantAssignmentStore(MemoryGlobalEquityGrantAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalEquityGrantAssignmentStore(
            inner,
            GlobalEquityGrantAssignmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<GlobalEquityGrantAssignment> FindGlobalEquityGrantAssignments(IGlobalEquityGrantAssignmentStore store, GlobalEquityGrantAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
