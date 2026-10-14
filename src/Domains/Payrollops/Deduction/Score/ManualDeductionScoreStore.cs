namespace Acme.Workforce.Payrollops.Deduction.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual deduction scores by code.</summary>
public interface IManualDeductionScoreStore
{
    ManualDeductionScore? Find(string code);
    void Save(ManualDeductionScore item);
    bool Remove(string code);
    List<ManualDeductionScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualDeductionScoreStore(Dictionary<string, ManualDeductionScore> items, List<string> order) : IManualDeductionScoreStore
{
    public Dictionary<string, ManualDeductionScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualDeductionScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualDeductionScore item)
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

    public List<ManualDeductionScore> List()
    {
        var outValue = new List<ManualDeductionScore>();
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
public sealed class CachedManualDeductionScoreStore(MemoryManualDeductionScoreStore inner, ManualDeductionScoreCache cache, int hits, int misses) : IManualDeductionScoreStore
{
    public MemoryManualDeductionScoreStore Inner { get; set; } = inner;
    public ManualDeductionScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualDeductionScore? Find(string code)
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

    public void Save(ManualDeductionScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualDeductionScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual deduction scores. Empty fields match all.</summary>
public sealed record ManualDeductionScoreQuery(string Label, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ManualDeductionScore item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualDeductionScore> Apply(List<ManualDeductionScore> items)
    {
        var matched = Collections.Filter(items, (ManualDeductionScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual deduction score store.</summary>
public static class ManualDeductionScoreStore
{
    public static IManualDeductionScoreStore NewMemoryManualDeductionScoreStore()
    {
        return new MemoryManualDeductionScoreStore(
            new Dictionary<string, ManualDeductionScore>(),
            new List<string>());
    }

    public static IManualDeductionScoreStore NewCachedManualDeductionScoreStore(MemoryManualDeductionScoreStore inner, int ttl, IClock clock)
    {
        return new CachedManualDeductionScoreStore(inner, ManualDeductionScoreCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ManualDeductionScore> FindManualDeductionScores(IManualDeductionScoreStore store, ManualDeductionScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
