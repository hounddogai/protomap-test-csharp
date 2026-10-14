namespace Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive license seat agreements by code.</summary>
public interface IAdaptiveLicenseSeatAgreementStore
{
    AdaptiveLicenseSeatAgreement? Find(string code);
    void Save(AdaptiveLicenseSeatAgreement item);
    bool Remove(string code);
    List<AdaptiveLicenseSeatAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveLicenseSeatAgreementStore(Dictionary<string, AdaptiveLicenseSeatAgreement> items, List<string> order) : IAdaptiveLicenseSeatAgreementStore
{
    public Dictionary<string, AdaptiveLicenseSeatAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveLicenseSeatAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveLicenseSeatAgreement item)
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

    public List<AdaptiveLicenseSeatAgreement> List()
    {
        var outValue = new List<AdaptiveLicenseSeatAgreement>();
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
public sealed class CachedAdaptiveLicenseSeatAgreementStore(MemoryAdaptiveLicenseSeatAgreementStore inner, AdaptiveLicenseSeatAgreementCache cache, int hits, int misses) : IAdaptiveLicenseSeatAgreementStore
{
    public MemoryAdaptiveLicenseSeatAgreementStore Inner { get; set; } = inner;
    public AdaptiveLicenseSeatAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveLicenseSeatAgreement? Find(string code)
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

    public void Save(AdaptiveLicenseSeatAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveLicenseSeatAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive license seat agreements. Empty fields match all.</summary>
public sealed record AdaptiveLicenseSeatAgreementQuery(string Source, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveLicenseSeatAgreement item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveLicenseSeatAgreement> Apply(List<AdaptiveLicenseSeatAgreement> items)
    {
        var matched = Collections.Filter(items, (AdaptiveLicenseSeatAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive license seat agreement store.</summary>
public static class AdaptiveLicenseSeatAgreementStore
{
    public static IAdaptiveLicenseSeatAgreementStore NewMemoryAdaptiveLicenseSeatAgreementStore()
    {
        return new MemoryAdaptiveLicenseSeatAgreementStore(
            new Dictionary<string, AdaptiveLicenseSeatAgreement>(),
            new List<string>());
    }

    public static IAdaptiveLicenseSeatAgreementStore NewCachedAdaptiveLicenseSeatAgreementStore(MemoryAdaptiveLicenseSeatAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveLicenseSeatAgreementStore(
            inner,
            AdaptiveLicenseSeatAgreementCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveLicenseSeatAgreement> FindAdaptiveLicenseSeatAgreements(IAdaptiveLicenseSeatAgreementStore store, AdaptiveLicenseSeatAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
