namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive position ledgers by code.</summary>
public interface IAdaptivePositionLedgerStore
{
    AdaptivePositionLedger? Find(string code);
    void Save(AdaptivePositionLedger item);
    bool Remove(string code);
    List<AdaptivePositionLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptivePositionLedgerStore(Dictionary<string, AdaptivePositionLedger> items, List<string> order) : IAdaptivePositionLedgerStore
{
    public Dictionary<string, AdaptivePositionLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptivePositionLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptivePositionLedger item)
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

    public List<AdaptivePositionLedger> List()
    {
        var outValue = new List<AdaptivePositionLedger>();
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
public sealed class CachedAdaptivePositionLedgerStore(MemoryAdaptivePositionLedgerStore inner, AdaptivePositionLedgerCache cache, int hits, int misses) : IAdaptivePositionLedgerStore
{
    public MemoryAdaptivePositionLedgerStore Inner { get; set; } = inner;
    public AdaptivePositionLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptivePositionLedger? Find(string code)
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

    public void Save(AdaptivePositionLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptivePositionLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive position ledgers. Empty fields match all.</summary>
public sealed record AdaptivePositionLedgerQuery(string Locale, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptivePositionLedger item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptivePositionLedger> Apply(List<AdaptivePositionLedger> items)
    {
        var matched = Collections.Filter(items, (AdaptivePositionLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive position ledger store.</summary>
public static class AdaptivePositionLedgerStore
{
    public static IAdaptivePositionLedgerStore NewMemoryAdaptivePositionLedgerStore()
    {
        return new MemoryAdaptivePositionLedgerStore(
            new Dictionary<string, AdaptivePositionLedger>(),
            new List<string>());
    }

    public static IAdaptivePositionLedgerStore NewCachedAdaptivePositionLedgerStore(MemoryAdaptivePositionLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptivePositionLedgerStore(
            inner,
            AdaptivePositionLedgerCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<AdaptivePositionLedger> FindAdaptivePositionLedgers(IAdaptivePositionLedgerStore store, AdaptivePositionLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
