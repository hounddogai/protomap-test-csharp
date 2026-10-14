namespace Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive offboarding ledgers by code.</summary>
public interface IAdaptiveOffboardingLedgerStore
{
    AdaptiveOffboardingLedger? Find(string code);
    void Save(AdaptiveOffboardingLedger item);
    bool Remove(string code);
    List<AdaptiveOffboardingLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveOffboardingLedgerStore(Dictionary<string, AdaptiveOffboardingLedger> items, List<string> order) : IAdaptiveOffboardingLedgerStore
{
    public Dictionary<string, AdaptiveOffboardingLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveOffboardingLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveOffboardingLedger item)
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

    public List<AdaptiveOffboardingLedger> List()
    {
        var outValue = new List<AdaptiveOffboardingLedger>();
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
public sealed class CachedAdaptiveOffboardingLedgerStore(MemoryAdaptiveOffboardingLedgerStore inner, AdaptiveOffboardingLedgerCache cache, int hits, int misses) : IAdaptiveOffboardingLedgerStore
{
    public MemoryAdaptiveOffboardingLedgerStore Inner { get; set; } = inner;
    public AdaptiveOffboardingLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveOffboardingLedger? Find(string code)
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

    public void Save(AdaptiveOffboardingLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveOffboardingLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive offboarding ledgers. Empty fields match all.</summary>
public sealed record AdaptiveOffboardingLedgerQuery(string Region, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveOffboardingLedger item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<AdaptiveOffboardingLedger> Apply(List<AdaptiveOffboardingLedger> items)
    {
        var matched = Collections.Filter(items, (AdaptiveOffboardingLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive offboarding ledger store.</summary>
public static class AdaptiveOffboardingLedgerStore
{
    public static IAdaptiveOffboardingLedgerStore NewMemoryAdaptiveOffboardingLedgerStore()
    {
        return new MemoryAdaptiveOffboardingLedgerStore(
            new Dictionary<string, AdaptiveOffboardingLedger>(),
            new List<string>());
    }

    public static IAdaptiveOffboardingLedgerStore NewCachedAdaptiveOffboardingLedgerStore(MemoryAdaptiveOffboardingLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveOffboardingLedgerStore(
            inner,
            AdaptiveOffboardingLedgerCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveOffboardingLedger> FindAdaptiveOffboardingLedgers(IAdaptiveOffboardingLedgerStore store, AdaptiveOffboardingLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
