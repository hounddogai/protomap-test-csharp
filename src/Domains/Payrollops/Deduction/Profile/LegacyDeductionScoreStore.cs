namespace Acme.Workforce.Payrollops.Deduction.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy deduction scores by code.</summary>
public interface ILegacyDeductionScoreStore
{
    LegacyDeductionScore? Find(string code);
    void Save(LegacyDeductionScore item);
    bool Remove(string code);
    List<LegacyDeductionScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyDeductionScoreStore(Dictionary<string, LegacyDeductionScore> items, List<string> order) : ILegacyDeductionScoreStore
{
    public Dictionary<string, LegacyDeductionScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyDeductionScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyDeductionScore item)
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

    public List<LegacyDeductionScore> List()
    {
        var outValue = new List<LegacyDeductionScore>();
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
public sealed class CachedLegacyDeductionScoreStore(MemoryLegacyDeductionScoreStore inner, LegacyDeductionScoreCache cache, int hits, int misses) : ILegacyDeductionScoreStore
{
    public MemoryLegacyDeductionScoreStore Inner { get; set; } = inner;
    public LegacyDeductionScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyDeductionScore? Find(string code)
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

    public void Save(LegacyDeductionScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyDeductionScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy deduction scores. Empty fields match all.</summary>
public sealed record LegacyDeductionScoreQuery(string Country, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyDeductionScore item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyDeductionScore> Apply(List<LegacyDeductionScore> items)
    {
        var matched = Collections.Filter(items, (LegacyDeductionScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy deduction score store.</summary>
public static class LegacyDeductionScoreStore
{
    public static ILegacyDeductionScoreStore NewMemoryLegacyDeductionScoreStore()
    {
        return new MemoryLegacyDeductionScoreStore(
            new Dictionary<string, LegacyDeductionScore>(),
            new List<string>());
    }

    public static ILegacyDeductionScoreStore NewCachedLegacyDeductionScoreStore(MemoryLegacyDeductionScoreStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyDeductionScoreStore(inner, LegacyDeductionScoreCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LegacyDeductionScore> FindLegacyDeductionScores(ILegacyDeductionScoreStore store, LegacyDeductionScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
