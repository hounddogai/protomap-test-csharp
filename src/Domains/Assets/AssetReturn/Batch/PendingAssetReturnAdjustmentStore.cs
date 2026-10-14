namespace Acme.Workforce.Assets.AssetReturn.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending asset return adjustments by code.</summary>
public interface IPendingAssetReturnAdjustmentStore
{
    PendingAssetReturnAdjustment? Find(string code);
    void Save(PendingAssetReturnAdjustment item);
    bool Remove(string code);
    List<PendingAssetReturnAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingAssetReturnAdjustmentStore(Dictionary<string, PendingAssetReturnAdjustment> items, List<string> order) : IPendingAssetReturnAdjustmentStore
{
    public Dictionary<string, PendingAssetReturnAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingAssetReturnAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingAssetReturnAdjustment item)
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

    public List<PendingAssetReturnAdjustment> List()
    {
        var outValue = new List<PendingAssetReturnAdjustment>();
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
public sealed class CachedPendingAssetReturnAdjustmentStore(MemoryPendingAssetReturnAdjustmentStore inner, PendingAssetReturnAdjustmentCache cache, int hits, int misses) : IPendingAssetReturnAdjustmentStore
{
    public MemoryPendingAssetReturnAdjustmentStore Inner { get; set; } = inner;
    public PendingAssetReturnAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingAssetReturnAdjustment? Find(string code)
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

    public void Save(PendingAssetReturnAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingAssetReturnAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending asset return adjustments. Empty fields match all.</summary>
public sealed record PendingAssetReturnAdjustmentQuery(string Channel, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(PendingAssetReturnAdjustment item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingAssetReturnAdjustment> Apply(List<PendingAssetReturnAdjustment> items)
    {
        var matched = Collections.Filter(items, (PendingAssetReturnAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending asset return adjustment store.</summary>
public static class PendingAssetReturnAdjustmentStore
{
    public static IPendingAssetReturnAdjustmentStore NewMemoryPendingAssetReturnAdjustmentStore()
    {
        return new MemoryPendingAssetReturnAdjustmentStore(
            new Dictionary<string, PendingAssetReturnAdjustment>(),
            new List<string>());
    }

    public static IPendingAssetReturnAdjustmentStore NewCachedPendingAssetReturnAdjustmentStore(MemoryPendingAssetReturnAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedPendingAssetReturnAdjustmentStore(
            inner,
            PendingAssetReturnAdjustmentCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PendingAssetReturnAdjustment> FindPendingAssetReturnAdjustments(IPendingAssetReturnAdjustmentStore store, PendingAssetReturnAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
