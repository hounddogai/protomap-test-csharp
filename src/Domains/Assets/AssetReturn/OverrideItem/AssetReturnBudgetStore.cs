namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return budgets by code.</summary>
public interface IAssetReturnBudgetStore
{
    AssetReturnBudget? Find(string code);
    void Save(AssetReturnBudget item);
    bool Remove(string code);
    List<AssetReturnBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnBudgetStore(Dictionary<string, AssetReturnBudget> items, List<string> order) : IAssetReturnBudgetStore
{
    public Dictionary<string, AssetReturnBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnBudget item)
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

    public List<AssetReturnBudget> List()
    {
        var outValue = new List<AssetReturnBudget>();
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
public sealed class CachedAssetReturnBudgetStore(MemoryAssetReturnBudgetStore inner, AssetReturnBudgetCache cache, int hits, int misses) : IAssetReturnBudgetStore
{
    public MemoryAssetReturnBudgetStore Inner { get; set; } = inner;
    public AssetReturnBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnBudget? Find(string code)
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

    public void Save(AssetReturnBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return budgets. Empty fields match all.</summary>
public sealed record AssetReturnBudgetQuery(string Team, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnBudget item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssetReturnBudget> Apply(List<AssetReturnBudget> items)
    {
        var matched = Collections.Filter(items, (AssetReturnBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return budget store.</summary>
public static class AssetReturnBudgetStore
{
    public static IAssetReturnBudgetStore NewMemoryAssetReturnBudgetStore()
    {
        return new MemoryAssetReturnBudgetStore(new Dictionary<string, AssetReturnBudget>(), new List<string>());
    }

    public static IAssetReturnBudgetStore NewCachedAssetReturnBudgetStore(MemoryAssetReturnBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnBudgetStore(inner, AssetReturnBudgetCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AssetReturnBudget> FindAssetReturnBudgets(IAssetReturnBudgetStore store, AssetReturnBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
