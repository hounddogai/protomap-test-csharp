namespace Acme.Workforce.Compliance.TrainingDue.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient training due tiers by code.</summary>
public interface ILenientTrainingDueTierStore
{
    LenientTrainingDueTier? Find(string code);
    void Save(LenientTrainingDueTier item);
    bool Remove(string code);
    List<LenientTrainingDueTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientTrainingDueTierStore(Dictionary<string, LenientTrainingDueTier> items, List<string> order) : ILenientTrainingDueTierStore
{
    public Dictionary<string, LenientTrainingDueTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientTrainingDueTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientTrainingDueTier item)
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

    public List<LenientTrainingDueTier> List()
    {
        var outValue = new List<LenientTrainingDueTier>();
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
public sealed class CachedLenientTrainingDueTierStore(MemoryLenientTrainingDueTierStore inner, LenientTrainingDueTierCache cache, int hits, int misses) : ILenientTrainingDueTierStore
{
    public MemoryLenientTrainingDueTierStore Inner { get; set; } = inner;
    public LenientTrainingDueTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientTrainingDueTier? Find(string code)
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

    public void Save(LenientTrainingDueTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientTrainingDueTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient training due tiers. Empty fields match all.</summary>
public sealed record LenientTrainingDueTierQuery(string Channel, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(LenientTrainingDueTier item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientTrainingDueTier> Apply(List<LenientTrainingDueTier> items)
    {
        var matched = Collections.Filter(items, (LenientTrainingDueTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient training due tier store.</summary>
public static class LenientTrainingDueTierStore
{
    public static ILenientTrainingDueTierStore NewMemoryLenientTrainingDueTierStore()
    {
        return new MemoryLenientTrainingDueTierStore(
            new Dictionary<string, LenientTrainingDueTier>(),
            new List<string>());
    }

    public static ILenientTrainingDueTierStore NewCachedLenientTrainingDueTierStore(MemoryLenientTrainingDueTierStore inner, int ttl, IClock clock)
    {
        return new CachedLenientTrainingDueTierStore(
            inner,
            LenientTrainingDueTierCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LenientTrainingDueTier> FindLenientTrainingDueTiers(ILenientTrainingDueTierStore store, LenientTrainingDueTierQuery query)
    {
        return query.Apply(store.List());
    }
}
