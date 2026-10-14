namespace Acme.Workforce.Contractors.Engagement.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted engagement agreements by code.</summary>
public interface IWeightedEngagementAgreementStore
{
    WeightedEngagementAgreement? Find(string code);
    void Save(WeightedEngagementAgreement item);
    bool Remove(string code);
    List<WeightedEngagementAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedEngagementAgreementStore(Dictionary<string, WeightedEngagementAgreement> items, List<string> order) : IWeightedEngagementAgreementStore
{
    public Dictionary<string, WeightedEngagementAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedEngagementAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedEngagementAgreement item)
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

    public List<WeightedEngagementAgreement> List()
    {
        var outValue = new List<WeightedEngagementAgreement>();
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
public sealed class CachedWeightedEngagementAgreementStore(MemoryWeightedEngagementAgreementStore inner, WeightedEngagementAgreementCache cache, int hits, int misses) : IWeightedEngagementAgreementStore
{
    public MemoryWeightedEngagementAgreementStore Inner { get; set; } = inner;
    public WeightedEngagementAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedEngagementAgreement? Find(string code)
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

    public void Save(WeightedEngagementAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedEngagementAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted engagement agreements. Empty fields match all.</summary>
public sealed record WeightedEngagementAgreementQuery(string Locale, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedEngagementAgreement item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedEngagementAgreement> Apply(List<WeightedEngagementAgreement> items)
    {
        var matched = Collections.Filter(items, (WeightedEngagementAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted engagement agreement store.</summary>
public static class WeightedEngagementAgreementStore
{
    public static IWeightedEngagementAgreementStore NewMemoryWeightedEngagementAgreementStore()
    {
        return new MemoryWeightedEngagementAgreementStore(
            new Dictionary<string, WeightedEngagementAgreement>(),
            new List<string>());
    }

    public static IWeightedEngagementAgreementStore NewCachedWeightedEngagementAgreementStore(MemoryWeightedEngagementAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedEngagementAgreementStore(
            inner,
            WeightedEngagementAgreementCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<WeightedEngagementAgreement> FindWeightedEngagementAgreements(IWeightedEngagementAgreementStore store, WeightedEngagementAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
