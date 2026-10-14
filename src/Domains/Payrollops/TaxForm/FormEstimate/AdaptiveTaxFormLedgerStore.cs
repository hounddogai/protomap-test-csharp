namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive tax form ledgers by code.</summary>
public interface IAdaptiveTaxFormLedgerStore
{
    AdaptiveTaxFormLedger? Find(string code);
    void Save(AdaptiveTaxFormLedger item);
    bool Remove(string code);
    List<AdaptiveTaxFormLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveTaxFormLedgerStore(Dictionary<string, AdaptiveTaxFormLedger> items, List<string> order) : IAdaptiveTaxFormLedgerStore
{
    public Dictionary<string, AdaptiveTaxFormLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveTaxFormLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveTaxFormLedger item)
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

    public List<AdaptiveTaxFormLedger> List()
    {
        var outValue = new List<AdaptiveTaxFormLedger>();
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
public sealed class CachedAdaptiveTaxFormLedgerStore(MemoryAdaptiveTaxFormLedgerStore inner, AdaptiveTaxFormLedgerCache cache, int hits, int misses) : IAdaptiveTaxFormLedgerStore
{
    public MemoryAdaptiveTaxFormLedgerStore Inner { get; set; } = inner;
    public AdaptiveTaxFormLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveTaxFormLedger? Find(string code)
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

    public void Save(AdaptiveTaxFormLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveTaxFormLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive tax form ledgers. Empty fields match all.</summary>
public sealed record AdaptiveTaxFormLedgerQuery(string Locale, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveTaxFormLedger item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<AdaptiveTaxFormLedger> Apply(List<AdaptiveTaxFormLedger> items)
    {
        var matched = Collections.Filter(items, (AdaptiveTaxFormLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive tax form ledger store.</summary>
public static class AdaptiveTaxFormLedgerStore
{
    public static IAdaptiveTaxFormLedgerStore NewMemoryAdaptiveTaxFormLedgerStore()
    {
        return new MemoryAdaptiveTaxFormLedgerStore(
            new Dictionary<string, AdaptiveTaxFormLedger>(),
            new List<string>());
    }

    public static IAdaptiveTaxFormLedgerStore NewCachedAdaptiveTaxFormLedgerStore(MemoryAdaptiveTaxFormLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveTaxFormLedgerStore(
            inner,
            AdaptiveTaxFormLedgerCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveTaxFormLedger> FindAdaptiveTaxFormLedgers(IAdaptiveTaxFormLedgerStore store, AdaptiveTaxFormLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
