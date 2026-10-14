namespace Acme.Workforce.Assets.AssetReturn.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped asset return assignments by code.</summary>
public interface ICappedAssetReturnAssignmentStore
{
    CappedAssetReturnAssignment? Find(string code);
    void Save(CappedAssetReturnAssignment item);
    bool Remove(string code);
    List<CappedAssetReturnAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedAssetReturnAssignmentStore(Dictionary<string, CappedAssetReturnAssignment> items, List<string> order) : ICappedAssetReturnAssignmentStore
{
    public Dictionary<string, CappedAssetReturnAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedAssetReturnAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedAssetReturnAssignment item)
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

    public List<CappedAssetReturnAssignment> List()
    {
        var outValue = new List<CappedAssetReturnAssignment>();
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
public sealed class CachedCappedAssetReturnAssignmentStore(MemoryCappedAssetReturnAssignmentStore inner, CappedAssetReturnAssignmentCache cache, int hits, int misses) : ICappedAssetReturnAssignmentStore
{
    public MemoryCappedAssetReturnAssignmentStore Inner { get; set; } = inner;
    public CappedAssetReturnAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedAssetReturnAssignment? Find(string code)
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

    public void Save(CappedAssetReturnAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedAssetReturnAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped asset return assignments. Empty fields match all.</summary>
public sealed record CappedAssetReturnAssignmentQuery(string Label, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(CappedAssetReturnAssignment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedAssetReturnAssignment> Apply(List<CappedAssetReturnAssignment> items)
    {
        var matched = Collections.Filter(items, (CappedAssetReturnAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped asset return assignment store.</summary>
public static class CappedAssetReturnAssignmentStore
{
    public static ICappedAssetReturnAssignmentStore NewMemoryCappedAssetReturnAssignmentStore()
    {
        return new MemoryCappedAssetReturnAssignmentStore(
            new Dictionary<string, CappedAssetReturnAssignment>(),
            new List<string>());
    }

    public static ICappedAssetReturnAssignmentStore NewCachedCappedAssetReturnAssignmentStore(MemoryCappedAssetReturnAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedCappedAssetReturnAssignmentStore(
            inner,
            CappedAssetReturnAssignmentCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<CappedAssetReturnAssignment> FindCappedAssetReturnAssignments(ICappedAssetReturnAssignmentStore store, CappedAssetReturnAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
