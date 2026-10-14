namespace Acme.Workforce.Workplace.AccessBadge.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback access badge agreements by code.</summary>
public interface IFallbackAccessBadgeAgreementStore
{
    FallbackAccessBadgeAgreement? Find(string code);
    void Save(FallbackAccessBadgeAgreement item);
    bool Remove(string code);
    List<FallbackAccessBadgeAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackAccessBadgeAgreementStore(Dictionary<string, FallbackAccessBadgeAgreement> items, List<string> order) : IFallbackAccessBadgeAgreementStore
{
    public Dictionary<string, FallbackAccessBadgeAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackAccessBadgeAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackAccessBadgeAgreement item)
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

    public List<FallbackAccessBadgeAgreement> List()
    {
        var outValue = new List<FallbackAccessBadgeAgreement>();
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
public sealed class CachedFallbackAccessBadgeAgreementStore(MemoryFallbackAccessBadgeAgreementStore inner, FallbackAccessBadgeAgreementCache cache, int hits, int misses) : IFallbackAccessBadgeAgreementStore
{
    public MemoryFallbackAccessBadgeAgreementStore Inner { get; set; } = inner;
    public FallbackAccessBadgeAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackAccessBadgeAgreement? Find(string code)
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

    public void Save(FallbackAccessBadgeAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackAccessBadgeAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback access badge agreements. Empty fields match all.</summary>
public sealed record FallbackAccessBadgeAgreementQuery(string Unit, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackAccessBadgeAgreement item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackAccessBadgeAgreement> Apply(List<FallbackAccessBadgeAgreement> items)
    {
        var matched = Collections.Filter(items, (FallbackAccessBadgeAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback access badge agreement store.</summary>
public static class FallbackAccessBadgeAgreementStore
{
    public static IFallbackAccessBadgeAgreementStore NewMemoryFallbackAccessBadgeAgreementStore()
    {
        return new MemoryFallbackAccessBadgeAgreementStore(
            new Dictionary<string, FallbackAccessBadgeAgreement>(),
            new List<string>());
    }

    public static IFallbackAccessBadgeAgreementStore NewCachedFallbackAccessBadgeAgreementStore(MemoryFallbackAccessBadgeAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackAccessBadgeAgreementStore(
            inner,
            FallbackAccessBadgeAgreementCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<FallbackAccessBadgeAgreement> FindFallbackAccessBadgeAgreements(IFallbackAccessBadgeAgreementStore store, FallbackAccessBadgeAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
