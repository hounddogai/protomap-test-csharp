namespace Acme.Workforce.Engagement.TownHall.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive town hall ledgers by code.</summary>
public interface IAdaptiveTownHallLedgerStore
{
    AdaptiveTownHallLedger? Find(string code);
    void Save(AdaptiveTownHallLedger item);
    bool Remove(string code);
    List<AdaptiveTownHallLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveTownHallLedgerStore(Dictionary<string, AdaptiveTownHallLedger> items, List<string> order) : IAdaptiveTownHallLedgerStore
{
    public Dictionary<string, AdaptiveTownHallLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveTownHallLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveTownHallLedger item)
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

    public List<AdaptiveTownHallLedger> List()
    {
        var outValue = new List<AdaptiveTownHallLedger>();
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
public sealed class CachedAdaptiveTownHallLedgerStore(MemoryAdaptiveTownHallLedgerStore inner, AdaptiveTownHallLedgerCache cache, int hits, int misses) : IAdaptiveTownHallLedgerStore
{
    public MemoryAdaptiveTownHallLedgerStore Inner { get; set; } = inner;
    public AdaptiveTownHallLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveTownHallLedger? Find(string code)
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

    public void Save(AdaptiveTownHallLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveTownHallLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive town hall ledgers. Empty fields match all.</summary>
public sealed record AdaptiveTownHallLedgerQuery(string Name, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveTownHallLedger item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveTownHallLedger> Apply(List<AdaptiveTownHallLedger> items)
    {
        var matched = Collections.Filter(items, (AdaptiveTownHallLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive town hall ledger store.</summary>
public static class AdaptiveTownHallLedgerStore
{
    public static IAdaptiveTownHallLedgerStore NewMemoryAdaptiveTownHallLedgerStore()
    {
        return new MemoryAdaptiveTownHallLedgerStore(
            new Dictionary<string, AdaptiveTownHallLedger>(),
            new List<string>());
    }

    public static IAdaptiveTownHallLedgerStore NewCachedAdaptiveTownHallLedgerStore(MemoryAdaptiveTownHallLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveTownHallLedgerStore(
            inner,
            AdaptiveTownHallLedgerCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveTownHallLedger> FindAdaptiveTownHallLedgers(IAdaptiveTownHallLedgerStore store, AdaptiveTownHallLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
