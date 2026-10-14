namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy training due variances by code.</summary>
public interface ILegacyTrainingDueVarianceStore
{
    LegacyTrainingDueVariance? Find(string code);
    void Save(LegacyTrainingDueVariance item);
    bool Remove(string code);
    List<LegacyTrainingDueVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyTrainingDueVarianceStore(Dictionary<string, LegacyTrainingDueVariance> items, List<string> order) : ILegacyTrainingDueVarianceStore
{
    public Dictionary<string, LegacyTrainingDueVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyTrainingDueVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyTrainingDueVariance item)
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

    public List<LegacyTrainingDueVariance> List()
    {
        var outValue = new List<LegacyTrainingDueVariance>();
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
public sealed class CachedLegacyTrainingDueVarianceStore(MemoryLegacyTrainingDueVarianceStore inner, LegacyTrainingDueVarianceCache cache, int hits, int misses) : ILegacyTrainingDueVarianceStore
{
    public MemoryLegacyTrainingDueVarianceStore Inner { get; set; } = inner;
    public LegacyTrainingDueVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyTrainingDueVariance? Find(string code)
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

    public void Save(LegacyTrainingDueVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyTrainingDueVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy training due variances. Empty fields match all.</summary>
public sealed record LegacyTrainingDueVarianceQuery(string Channel, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyTrainingDueVariance item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyTrainingDueVariance> Apply(List<LegacyTrainingDueVariance> items)
    {
        var matched = Collections.Filter(items, (LegacyTrainingDueVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy training due variance store.</summary>
public static class LegacyTrainingDueVarianceStore
{
    public static ILegacyTrainingDueVarianceStore NewMemoryLegacyTrainingDueVarianceStore()
    {
        return new MemoryLegacyTrainingDueVarianceStore(
            new Dictionary<string, LegacyTrainingDueVariance>(),
            new List<string>());
    }

    public static ILegacyTrainingDueVarianceStore NewCachedLegacyTrainingDueVarianceStore(MemoryLegacyTrainingDueVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyTrainingDueVarianceStore(
            inner,
            LegacyTrainingDueVarianceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LegacyTrainingDueVariance> FindLegacyTrainingDueVariances(ILegacyTrainingDueVarianceStore store, LegacyTrainingDueVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
