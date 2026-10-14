namespace Acme.Workforce.Compensation.Adjustment.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment agreements by code.</summary>
public interface IAdjustmentAgreementStore
{
    AdjustmentAgreement? Find(string code);
    void Save(AdjustmentAgreement item);
    bool Remove(string code);
    List<AdjustmentAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentAgreementStore(Dictionary<string, AdjustmentAgreement> items, List<string> order) : IAdjustmentAgreementStore
{
    public Dictionary<string, AdjustmentAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentAgreement item)
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

    public List<AdjustmentAgreement> List()
    {
        var outValue = new List<AdjustmentAgreement>();
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
public sealed class CachedAdjustmentAgreementStore(MemoryAdjustmentAgreementStore inner, AdjustmentAgreementCache cache, int hits, int misses) : IAdjustmentAgreementStore
{
    public MemoryAdjustmentAgreementStore Inner { get; set; } = inner;
    public AdjustmentAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentAgreement? Find(string code)
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

    public void Save(AdjustmentAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment agreements. Empty fields match all.</summary>
public sealed record AdjustmentAgreementQuery(string Country, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentAgreement item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdjustmentAgreement> Apply(List<AdjustmentAgreement> items)
    {
        var matched = Collections.Filter(items, (AdjustmentAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment agreement store.</summary>
public static class AdjustmentAgreementStore
{
    public static IAdjustmentAgreementStore NewMemoryAdjustmentAgreementStore()
    {
        return new MemoryAdjustmentAgreementStore(new Dictionary<string, AdjustmentAgreement>(), new List<string>());
    }

    public static IAdjustmentAgreementStore NewCachedAdjustmentAgreementStore(MemoryAdjustmentAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentAgreementStore(inner, AdjustmentAgreementCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AdjustmentAgreement> FindAdjustmentAgreements(IAdjustmentAgreementStore store, AdjustmentAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
