namespace Acme.Workforce.Absence.Balance.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback balance cycles by code.</summary>
public interface IFallbackBalanceCycleStore
{
    FallbackBalanceCycle? Find(string code);
    void Save(FallbackBalanceCycle item);
    bool Remove(string code);
    List<FallbackBalanceCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackBalanceCycleStore(Dictionary<string, FallbackBalanceCycle> items, List<string> order) : IFallbackBalanceCycleStore
{
    public Dictionary<string, FallbackBalanceCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackBalanceCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackBalanceCycle item)
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

    public List<FallbackBalanceCycle> List()
    {
        var outValue = new List<FallbackBalanceCycle>();
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
public sealed class CachedFallbackBalanceCycleStore(MemoryFallbackBalanceCycleStore inner, FallbackBalanceCycleCache cache, int hits, int misses) : IFallbackBalanceCycleStore
{
    public MemoryFallbackBalanceCycleStore Inner { get; set; } = inner;
    public FallbackBalanceCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackBalanceCycle? Find(string code)
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

    public void Save(FallbackBalanceCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackBalanceCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback balance cycles. Empty fields match all.</summary>
public sealed record FallbackBalanceCycleQuery(string Label, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackBalanceCycle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<FallbackBalanceCycle> Apply(List<FallbackBalanceCycle> items)
    {
        var matched = Collections.Filter(items, (FallbackBalanceCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback balance cycle store.</summary>
public static class FallbackBalanceCycleStore
{
    public static IFallbackBalanceCycleStore NewMemoryFallbackBalanceCycleStore()
    {
        return new MemoryFallbackBalanceCycleStore(
            new Dictionary<string, FallbackBalanceCycle>(),
            new List<string>());
    }

    public static IFallbackBalanceCycleStore NewCachedFallbackBalanceCycleStore(MemoryFallbackBalanceCycleStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackBalanceCycleStore(inner, FallbackBalanceCycleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<FallbackBalanceCycle> FindFallbackBalanceCycles(IFallbackBalanceCycleStore store, FallbackBalanceCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
