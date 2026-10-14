namespace Acme.Workforce.Contractors.Engagement.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped engagement agreements by code.</summary>
public interface ICappedEngagementAgreementStore
{
    CappedEngagementAgreement? Find(string code);
    void Save(CappedEngagementAgreement item);
    bool Remove(string code);
    List<CappedEngagementAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedEngagementAgreementStore(Dictionary<string, CappedEngagementAgreement> items, List<string> order) : ICappedEngagementAgreementStore
{
    public Dictionary<string, CappedEngagementAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedEngagementAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedEngagementAgreement item)
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

    public List<CappedEngagementAgreement> List()
    {
        var outValue = new List<CappedEngagementAgreement>();
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
public sealed class CachedCappedEngagementAgreementStore(MemoryCappedEngagementAgreementStore inner, CappedEngagementAgreementCache cache, int hits, int misses) : ICappedEngagementAgreementStore
{
    public MemoryCappedEngagementAgreementStore Inner { get; set; } = inner;
    public CappedEngagementAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedEngagementAgreement? Find(string code)
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

    public void Save(CappedEngagementAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedEngagementAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped engagement agreements. Empty fields match all.</summary>
public sealed record CappedEngagementAgreementQuery(string Name, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(CappedEngagementAgreement item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedEngagementAgreement> Apply(List<CappedEngagementAgreement> items)
    {
        var matched = Collections.Filter(items, (CappedEngagementAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped engagement agreement store.</summary>
public static class CappedEngagementAgreementStore
{
    public static ICappedEngagementAgreementStore NewMemoryCappedEngagementAgreementStore()
    {
        return new MemoryCappedEngagementAgreementStore(
            new Dictionary<string, CappedEngagementAgreement>(),
            new List<string>());
    }

    public static ICappedEngagementAgreementStore NewCachedCappedEngagementAgreementStore(MemoryCappedEngagementAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedCappedEngagementAgreementStore(
            inner,
            CappedEngagementAgreementCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<CappedEngagementAgreement> FindCappedEngagementAgreements(ICappedEngagementAgreementStore store, CappedEngagementAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
