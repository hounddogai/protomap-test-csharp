namespace Acme.Workforce.Assets.Assignment.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global assignment adjustments by code.</summary>
public interface IGlobalAssignmentAdjustmentStore
{
    GlobalAssignmentAdjustment? Find(string code);
    void Save(GlobalAssignmentAdjustment item);
    bool Remove(string code);
    List<GlobalAssignmentAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalAssignmentAdjustmentStore(Dictionary<string, GlobalAssignmentAdjustment> items, List<string> order) : IGlobalAssignmentAdjustmentStore
{
    public Dictionary<string, GlobalAssignmentAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalAssignmentAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalAssignmentAdjustment item)
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

    public List<GlobalAssignmentAdjustment> List()
    {
        var outValue = new List<GlobalAssignmentAdjustment>();
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
public sealed class CachedGlobalAssignmentAdjustmentStore(MemoryGlobalAssignmentAdjustmentStore inner, GlobalAssignmentAdjustmentCache cache, int hits, int misses) : IGlobalAssignmentAdjustmentStore
{
    public MemoryGlobalAssignmentAdjustmentStore Inner { get; set; } = inner;
    public GlobalAssignmentAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalAssignmentAdjustment? Find(string code)
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

    public void Save(GlobalAssignmentAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalAssignmentAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global assignment adjustments. Empty fields match all.</summary>
public sealed record GlobalAssignmentAdjustmentQuery(string Label, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalAssignmentAdjustment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalAssignmentAdjustment> Apply(List<GlobalAssignmentAdjustment> items)
    {
        var matched = Collections.Filter(items, (GlobalAssignmentAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global assignment adjustment store.</summary>
public static class GlobalAssignmentAdjustmentStore
{
    public static IGlobalAssignmentAdjustmentStore NewMemoryGlobalAssignmentAdjustmentStore()
    {
        return new MemoryGlobalAssignmentAdjustmentStore(
            new Dictionary<string, GlobalAssignmentAdjustment>(),
            new List<string>());
    }

    public static IGlobalAssignmentAdjustmentStore NewCachedGlobalAssignmentAdjustmentStore(MemoryGlobalAssignmentAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalAssignmentAdjustmentStore(
            inner,
            GlobalAssignmentAdjustmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<GlobalAssignmentAdjustment> FindGlobalAssignmentAdjustments(IGlobalAssignmentAdjustmentStore store, GlobalAssignmentAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
