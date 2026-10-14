namespace Acme.Workforce.Assets.AssetReturn.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return plans by code.</summary>
public interface IAssetReturnPlanStore
{
    AssetReturnPlan? Find(string code);
    void Save(AssetReturnPlan item);
    bool Remove(string code);
    List<AssetReturnPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnPlanStore(Dictionary<string, AssetReturnPlan> items, List<string> order) : IAssetReturnPlanStore
{
    public Dictionary<string, AssetReturnPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnPlan item)
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

    public List<AssetReturnPlan> List()
    {
        var outValue = new List<AssetReturnPlan>();
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
public sealed class CachedAssetReturnPlanStore(MemoryAssetReturnPlanStore inner, AssetReturnPlanCache cache, int hits, int misses) : IAssetReturnPlanStore
{
    public MemoryAssetReturnPlanStore Inner { get; set; } = inner;
    public AssetReturnPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnPlan? Find(string code)
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

    public void Save(AssetReturnPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return plans. Empty fields match all.</summary>
public sealed record AssetReturnPlanQuery(string Title, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnPlan item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssetReturnPlan> Apply(List<AssetReturnPlan> items)
    {
        var matched = Collections.Filter(items, (AssetReturnPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return plan store.</summary>
public static class AssetReturnPlanStore
{
    public static IAssetReturnPlanStore NewMemoryAssetReturnPlanStore()
    {
        return new MemoryAssetReturnPlanStore(new Dictionary<string, AssetReturnPlan>(), new List<string>());
    }

    public static IAssetReturnPlanStore NewCachedAssetReturnPlanStore(MemoryAssetReturnPlanStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnPlanStore(inner, AssetReturnPlanCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AssetReturnPlan> FindAssetReturnPlans(IAssetReturnPlanStore store, AssetReturnPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
