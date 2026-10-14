namespace Acme.Workforce.Talent.Mobility.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict mobility overrides by code.</summary>
public interface IStrictMobilityOverrideStore
{
    StrictMobilityOverride? Find(string code);
    void Save(StrictMobilityOverride item);
    bool Remove(string code);
    List<StrictMobilityOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictMobilityOverrideStore(Dictionary<string, StrictMobilityOverride> items, List<string> order) : IStrictMobilityOverrideStore
{
    public Dictionary<string, StrictMobilityOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictMobilityOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictMobilityOverride item)
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

    public List<StrictMobilityOverride> List()
    {
        var outValue = new List<StrictMobilityOverride>();
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
public sealed class CachedStrictMobilityOverrideStore(MemoryStrictMobilityOverrideStore inner, StrictMobilityOverrideCache cache, int hits, int misses) : IStrictMobilityOverrideStore
{
    public MemoryStrictMobilityOverrideStore Inner { get; set; } = inner;
    public StrictMobilityOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictMobilityOverride? Find(string code)
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

    public void Save(StrictMobilityOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictMobilityOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict mobility overrides. Empty fields match all.</summary>
public sealed record StrictMobilityOverrideQuery(string Segment, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(StrictMobilityOverride item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictMobilityOverride> Apply(List<StrictMobilityOverride> items)
    {
        var matched = Collections.Filter(items, (StrictMobilityOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict mobility override store.</summary>
public static class StrictMobilityOverrideStore
{
    public static IStrictMobilityOverrideStore NewMemoryStrictMobilityOverrideStore()
    {
        return new MemoryStrictMobilityOverrideStore(
            new Dictionary<string, StrictMobilityOverride>(),
            new List<string>());
    }

    public static IStrictMobilityOverrideStore NewCachedStrictMobilityOverrideStore(MemoryStrictMobilityOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedStrictMobilityOverrideStore(
            inner,
            StrictMobilityOverrideCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<StrictMobilityOverride> FindStrictMobilityOverrides(IStrictMobilityOverrideStore store, StrictMobilityOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
