namespace Acme.Workforce.Assets.AssetReturn.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped asset return cycles by code.</summary>
public interface ICappedAssetReturnCycleStore
{
    CappedAssetReturnCycle? Find(string code);
    void Save(CappedAssetReturnCycle item);
    bool Remove(string code);
    List<CappedAssetReturnCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedAssetReturnCycleStore(Dictionary<string, CappedAssetReturnCycle> items, List<string> order) : ICappedAssetReturnCycleStore
{
    public Dictionary<string, CappedAssetReturnCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedAssetReturnCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedAssetReturnCycle item)
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

    public List<CappedAssetReturnCycle> List()
    {
        var outValue = new List<CappedAssetReturnCycle>();
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
public sealed class CachedCappedAssetReturnCycleStore(MemoryCappedAssetReturnCycleStore inner, CappedAssetReturnCycleCache cache, int hits, int misses) : ICappedAssetReturnCycleStore
{
    public MemoryCappedAssetReturnCycleStore Inner { get; set; } = inner;
    public CappedAssetReturnCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedAssetReturnCycle? Find(string code)
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

    public void Save(CappedAssetReturnCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedAssetReturnCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped asset return cycles. Empty fields match all.</summary>
public sealed record CappedAssetReturnCycleQuery(string Team, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(CappedAssetReturnCycle item)
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

    public List<CappedAssetReturnCycle> Apply(List<CappedAssetReturnCycle> items)
    {
        var matched = Collections.Filter(items, (CappedAssetReturnCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped asset return cycle store.</summary>
public static class CappedAssetReturnCycleStore
{
    public static ICappedAssetReturnCycleStore NewMemoryCappedAssetReturnCycleStore()
    {
        return new MemoryCappedAssetReturnCycleStore(
            new Dictionary<string, CappedAssetReturnCycle>(),
            new List<string>());
    }

    public static ICappedAssetReturnCycleStore NewCachedCappedAssetReturnCycleStore(MemoryCappedAssetReturnCycleStore inner, int ttl, IClock clock)
    {
        return new CachedCappedAssetReturnCycleStore(
            inner,
            CappedAssetReturnCycleCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<CappedAssetReturnCycle> FindCappedAssetReturnCycles(ICappedAssetReturnCycleStore store, CappedAssetReturnCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
