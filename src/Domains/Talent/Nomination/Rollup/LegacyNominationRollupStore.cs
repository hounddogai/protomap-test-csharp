namespace Acme.Workforce.Talent.Nomination.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy nomination rollups by code.</summary>
public interface ILegacyNominationRollupStore
{
    LegacyNominationRollup? Find(string code);
    void Save(LegacyNominationRollup item);
    bool Remove(string code);
    List<LegacyNominationRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyNominationRollupStore(Dictionary<string, LegacyNominationRollup> items, List<string> order) : ILegacyNominationRollupStore
{
    public Dictionary<string, LegacyNominationRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyNominationRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyNominationRollup item)
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

    public List<LegacyNominationRollup> List()
    {
        var outValue = new List<LegacyNominationRollup>();
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
public sealed class CachedLegacyNominationRollupStore(MemoryLegacyNominationRollupStore inner, LegacyNominationRollupCache cache, int hits, int misses) : ILegacyNominationRollupStore
{
    public MemoryLegacyNominationRollupStore Inner { get; set; } = inner;
    public LegacyNominationRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyNominationRollup? Find(string code)
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

    public void Save(LegacyNominationRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyNominationRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy nomination rollups. Empty fields match all.</summary>
public sealed record LegacyNominationRollupQuery(string Name, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyNominationRollup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyNominationRollup> Apply(List<LegacyNominationRollup> items)
    {
        var matched = Collections.Filter(items, (LegacyNominationRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy nomination rollup store.</summary>
public static class LegacyNominationRollupStore
{
    public static ILegacyNominationRollupStore NewMemoryLegacyNominationRollupStore()
    {
        return new MemoryLegacyNominationRollupStore(
            new Dictionary<string, LegacyNominationRollup>(),
            new List<string>());
    }

    public static ILegacyNominationRollupStore NewCachedLegacyNominationRollupStore(MemoryLegacyNominationRollupStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyNominationRollupStore(
            inner,
            LegacyNominationRollupCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LegacyNominationRollup> FindLegacyNominationRollups(ILegacyNominationRollupStore store, LegacyNominationRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
