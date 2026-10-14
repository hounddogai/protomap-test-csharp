namespace Acme.Workforce.Assets.AssetReturn.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered asset return ledgers by code.</summary>
public interface ITieredAssetReturnLedgerStore
{
    TieredAssetReturnLedger? Find(string code);
    void Save(TieredAssetReturnLedger item);
    bool Remove(string code);
    List<TieredAssetReturnLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredAssetReturnLedgerStore(Dictionary<string, TieredAssetReturnLedger> items, List<string> order) : ITieredAssetReturnLedgerStore
{
    public Dictionary<string, TieredAssetReturnLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredAssetReturnLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredAssetReturnLedger item)
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

    public List<TieredAssetReturnLedger> List()
    {
        var outValue = new List<TieredAssetReturnLedger>();
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
public sealed class CachedTieredAssetReturnLedgerStore(MemoryTieredAssetReturnLedgerStore inner, TieredAssetReturnLedgerCache cache, int hits, int misses) : ITieredAssetReturnLedgerStore
{
    public MemoryTieredAssetReturnLedgerStore Inner { get; set; } = inner;
    public TieredAssetReturnLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredAssetReturnLedger? Find(string code)
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

    public void Save(TieredAssetReturnLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredAssetReturnLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered asset return ledgers. Empty fields match all.</summary>
public sealed record TieredAssetReturnLedgerQuery(string Segment, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(TieredAssetReturnLedger item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredAssetReturnLedger> Apply(List<TieredAssetReturnLedger> items)
    {
        var matched = Collections.Filter(items, (TieredAssetReturnLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered asset return ledger store.</summary>
public static class TieredAssetReturnLedgerStore
{
    public static ITieredAssetReturnLedgerStore NewMemoryTieredAssetReturnLedgerStore()
    {
        return new MemoryTieredAssetReturnLedgerStore(
            new Dictionary<string, TieredAssetReturnLedger>(),
            new List<string>());
    }

    public static ITieredAssetReturnLedgerStore NewCachedTieredAssetReturnLedgerStore(MemoryTieredAssetReturnLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedTieredAssetReturnLedgerStore(
            inner,
            TieredAssetReturnLedgerCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<TieredAssetReturnLedger> FindTieredAssetReturnLedgers(ITieredAssetReturnLedgerStore store, TieredAssetReturnLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
