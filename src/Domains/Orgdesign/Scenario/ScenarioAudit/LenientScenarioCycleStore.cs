namespace Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient scenario cycles by code.</summary>
public interface ILenientScenarioCycleStore
{
    LenientScenarioCycle? Find(string code);
    void Save(LenientScenarioCycle item);
    bool Remove(string code);
    List<LenientScenarioCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientScenarioCycleStore(Dictionary<string, LenientScenarioCycle> items, List<string> order) : ILenientScenarioCycleStore
{
    public Dictionary<string, LenientScenarioCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientScenarioCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientScenarioCycle item)
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

    public List<LenientScenarioCycle> List()
    {
        var outValue = new List<LenientScenarioCycle>();
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
public sealed class CachedLenientScenarioCycleStore(MemoryLenientScenarioCycleStore inner, LenientScenarioCycleCache cache, int hits, int misses) : ILenientScenarioCycleStore
{
    public MemoryLenientScenarioCycleStore Inner { get; set; } = inner;
    public LenientScenarioCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientScenarioCycle? Find(string code)
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

    public void Save(LenientScenarioCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientScenarioCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient scenario cycles. Empty fields match all.</summary>
public sealed record LenientScenarioCycleQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(LenientScenarioCycle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientScenarioCycle> Apply(List<LenientScenarioCycle> items)
    {
        var matched = Collections.Filter(items, (LenientScenarioCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient scenario cycle store.</summary>
public static class LenientScenarioCycleStore
{
    public static ILenientScenarioCycleStore NewMemoryLenientScenarioCycleStore()
    {
        return new MemoryLenientScenarioCycleStore(
            new Dictionary<string, LenientScenarioCycle>(),
            new List<string>());
    }

    public static ILenientScenarioCycleStore NewCachedLenientScenarioCycleStore(MemoryLenientScenarioCycleStore inner, int ttl, IClock clock)
    {
        return new CachedLenientScenarioCycleStore(inner, LenientScenarioCycleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LenientScenarioCycle> FindLenientScenarioCycles(ILenientScenarioCycleStore store, LenientScenarioCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
