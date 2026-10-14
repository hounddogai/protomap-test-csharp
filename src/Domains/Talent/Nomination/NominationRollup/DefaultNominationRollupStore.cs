namespace Acme.Workforce.Talent.Nomination.NominationRollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default nomination rollups by code.</summary>
public interface IDefaultNominationRollupStore
{
    DefaultNominationRollup? Find(string code);
    void Save(DefaultNominationRollup item);
    bool Remove(string code);
    List<DefaultNominationRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultNominationRollupStore(Dictionary<string, DefaultNominationRollup> items, List<string> order) : IDefaultNominationRollupStore
{
    public Dictionary<string, DefaultNominationRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultNominationRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultNominationRollup item)
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

    public List<DefaultNominationRollup> List()
    {
        var outValue = new List<DefaultNominationRollup>();
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
public sealed class CachedDefaultNominationRollupStore(MemoryDefaultNominationRollupStore inner, DefaultNominationRollupCache cache, int hits, int misses) : IDefaultNominationRollupStore
{
    public MemoryDefaultNominationRollupStore Inner { get; set; } = inner;
    public DefaultNominationRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultNominationRollup? Find(string code)
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

    public void Save(DefaultNominationRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultNominationRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default nomination rollups. Empty fields match all.</summary>
public sealed record DefaultNominationRollupQuery(string Channel, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultNominationRollup item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultNominationRollup> Apply(List<DefaultNominationRollup> items)
    {
        var matched = Collections.Filter(items, (DefaultNominationRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default nomination rollup store.</summary>
public static class DefaultNominationRollupStore
{
    public static IDefaultNominationRollupStore NewMemoryDefaultNominationRollupStore()
    {
        return new MemoryDefaultNominationRollupStore(
            new Dictionary<string, DefaultNominationRollup>(),
            new List<string>());
    }

    public static IDefaultNominationRollupStore NewCachedDefaultNominationRollupStore(MemoryDefaultNominationRollupStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultNominationRollupStore(
            inner,
            DefaultNominationRollupCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<DefaultNominationRollup> FindDefaultNominationRollups(IDefaultNominationRollupStore store, DefaultNominationRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
