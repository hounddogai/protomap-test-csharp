namespace Acme.Workforce.Assets.AssetReturn.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling asset return assignments by code.</summary>
public interface IRollingAssetReturnAssignmentStore
{
    RollingAssetReturnAssignment? Find(string code);
    void Save(RollingAssetReturnAssignment item);
    bool Remove(string code);
    List<RollingAssetReturnAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingAssetReturnAssignmentStore(Dictionary<string, RollingAssetReturnAssignment> items, List<string> order) : IRollingAssetReturnAssignmentStore
{
    public Dictionary<string, RollingAssetReturnAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingAssetReturnAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingAssetReturnAssignment item)
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

    public List<RollingAssetReturnAssignment> List()
    {
        var outValue = new List<RollingAssetReturnAssignment>();
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
public sealed class CachedRollingAssetReturnAssignmentStore(MemoryRollingAssetReturnAssignmentStore inner, RollingAssetReturnAssignmentCache cache, int hits, int misses) : IRollingAssetReturnAssignmentStore
{
    public MemoryRollingAssetReturnAssignmentStore Inner { get; set; } = inner;
    public RollingAssetReturnAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingAssetReturnAssignment? Find(string code)
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

    public void Save(RollingAssetReturnAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingAssetReturnAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling asset return assignments. Empty fields match all.</summary>
public sealed record RollingAssetReturnAssignmentQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(RollingAssetReturnAssignment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingAssetReturnAssignment> Apply(List<RollingAssetReturnAssignment> items)
    {
        var matched = Collections.Filter(items, (RollingAssetReturnAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling asset return assignment store.</summary>
public static class RollingAssetReturnAssignmentStore
{
    public static IRollingAssetReturnAssignmentStore NewMemoryRollingAssetReturnAssignmentStore()
    {
        return new MemoryRollingAssetReturnAssignmentStore(
            new Dictionary<string, RollingAssetReturnAssignment>(),
            new List<string>());
    }

    public static IRollingAssetReturnAssignmentStore NewCachedRollingAssetReturnAssignmentStore(MemoryRollingAssetReturnAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedRollingAssetReturnAssignmentStore(
            inner,
            RollingAssetReturnAssignmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RollingAssetReturnAssignment> FindRollingAssetReturnAssignments(IRollingAssetReturnAssignmentStore store, RollingAssetReturnAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
