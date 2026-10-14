namespace Acme.Workforce.Assets.AssetReturn.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return ledgers by code.</summary>
public interface IAssetReturnLedgerStore
{
    AssetReturnLedger? Find(string code);
    void Save(AssetReturnLedger item);
    bool Remove(string code);
    List<AssetReturnLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnLedgerStore(Dictionary<string, AssetReturnLedger> items, List<string> order) : IAssetReturnLedgerStore
{
    public Dictionary<string, AssetReturnLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnLedger item)
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

    public List<AssetReturnLedger> List()
    {
        var outValue = new List<AssetReturnLedger>();
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
public sealed class CachedAssetReturnLedgerStore(MemoryAssetReturnLedgerStore inner, AssetReturnLedgerCache cache, int hits, int misses) : IAssetReturnLedgerStore
{
    public MemoryAssetReturnLedgerStore Inner { get; set; } = inner;
    public AssetReturnLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnLedger? Find(string code)
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

    public void Save(AssetReturnLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return ledgers. Empty fields match all.</summary>
public sealed record AssetReturnLedgerQuery(string Title, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnLedger item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssetReturnLedger> Apply(List<AssetReturnLedger> items)
    {
        var matched = Collections.Filter(items, (AssetReturnLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return ledger store.</summary>
public static class AssetReturnLedgerStore
{
    public static IAssetReturnLedgerStore NewMemoryAssetReturnLedgerStore()
    {
        return new MemoryAssetReturnLedgerStore(new Dictionary<string, AssetReturnLedger>(), new List<string>());
    }

    public static IAssetReturnLedgerStore NewCachedAssetReturnLedgerStore(MemoryAssetReturnLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnLedgerStore(inner, AssetReturnLedgerCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AssetReturnLedger> FindAssetReturnLedgers(IAssetReturnLedgerStore store, AssetReturnLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
