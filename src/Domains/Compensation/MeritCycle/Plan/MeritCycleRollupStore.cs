namespace Acme.Workforce.Compensation.MeritCycle.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle rollups by code.</summary>
public interface IMeritCycleRollupStore
{
    MeritCycleRollup? Find(string code);
    void Save(MeritCycleRollup item);
    bool Remove(string code);
    List<MeritCycleRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleRollupStore(Dictionary<string, MeritCycleRollup> items, List<string> order) : IMeritCycleRollupStore
{
    public Dictionary<string, MeritCycleRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleRollup item)
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

    public List<MeritCycleRollup> List()
    {
        var outValue = new List<MeritCycleRollup>();
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
public sealed class CachedMeritCycleRollupStore(MemoryMeritCycleRollupStore inner, MeritCycleRollupCache cache, int hits, int misses) : IMeritCycleRollupStore
{
    public MemoryMeritCycleRollupStore Inner { get; set; } = inner;
    public MeritCycleRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleRollup? Find(string code)
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

    public void Save(MeritCycleRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle rollups. Empty fields match all.</summary>
public sealed record MeritCycleRollupQuery(string Channel, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleRollup item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleRollup> Apply(List<MeritCycleRollup> items)
    {
        var matched = Collections.Filter(items, (MeritCycleRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle rollup store.</summary>
public static class MeritCycleRollupStore
{
    public static IMeritCycleRollupStore NewMemoryMeritCycleRollupStore()
    {
        return new MemoryMeritCycleRollupStore(new Dictionary<string, MeritCycleRollup>(), new List<string>());
    }

    public static IMeritCycleRollupStore NewCachedMeritCycleRollupStore(MemoryMeritCycleRollupStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleRollupStore(inner, MeritCycleRollupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<MeritCycleRollup> FindMeritCycleRollups(IMeritCycleRollupStore store, MeritCycleRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
