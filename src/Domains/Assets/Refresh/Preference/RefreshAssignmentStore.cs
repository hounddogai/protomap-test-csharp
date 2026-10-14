namespace Acme.Workforce.Assets.Refresh.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh assignments by code.</summary>
public interface IRefreshAssignmentStore
{
    RefreshAssignment? Find(string code);
    void Save(RefreshAssignment item);
    bool Remove(string code);
    List<RefreshAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshAssignmentStore(Dictionary<string, RefreshAssignment> items, List<string> order) : IRefreshAssignmentStore
{
    public Dictionary<string, RefreshAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshAssignment item)
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

    public List<RefreshAssignment> List()
    {
        var outValue = new List<RefreshAssignment>();
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
public sealed class CachedRefreshAssignmentStore(MemoryRefreshAssignmentStore inner, RefreshAssignmentCache cache, int hits, int misses) : IRefreshAssignmentStore
{
    public MemoryRefreshAssignmentStore Inner { get; set; } = inner;
    public RefreshAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshAssignment? Find(string code)
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

    public void Save(RefreshAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh assignments. Empty fields match all.</summary>
public sealed record RefreshAssignmentQuery(string Team, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshAssignment item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<RefreshAssignment> Apply(List<RefreshAssignment> items)
    {
        var matched = Collections.Filter(items, (RefreshAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh assignment store.</summary>
public static class RefreshAssignmentStore
{
    public static IRefreshAssignmentStore NewMemoryRefreshAssignmentStore()
    {
        return new MemoryRefreshAssignmentStore(new Dictionary<string, RefreshAssignment>(), new List<string>());
    }

    public static IRefreshAssignmentStore NewCachedRefreshAssignmentStore(MemoryRefreshAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshAssignmentStore(inner, RefreshAssignmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RefreshAssignment> FindRefreshAssignments(IRefreshAssignmentStore store, RefreshAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
