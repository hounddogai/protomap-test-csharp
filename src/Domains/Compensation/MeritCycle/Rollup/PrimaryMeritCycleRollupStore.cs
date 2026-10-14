namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary merit cycle rollups by code.</summary>
public interface IPrimaryMeritCycleRollupStore
{
    PrimaryMeritCycleRollup? Find(string code);
    void Save(PrimaryMeritCycleRollup item);
    bool Remove(string code);
    List<PrimaryMeritCycleRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryMeritCycleRollupStore(Dictionary<string, PrimaryMeritCycleRollup> items, List<string> order) : IPrimaryMeritCycleRollupStore
{
    public Dictionary<string, PrimaryMeritCycleRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryMeritCycleRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryMeritCycleRollup item)
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

    public List<PrimaryMeritCycleRollup> List()
    {
        var outValue = new List<PrimaryMeritCycleRollup>();
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
public sealed class CachedPrimaryMeritCycleRollupStore(MemoryPrimaryMeritCycleRollupStore inner, PrimaryMeritCycleRollupCache cache, int hits, int misses) : IPrimaryMeritCycleRollupStore
{
    public MemoryPrimaryMeritCycleRollupStore Inner { get; set; } = inner;
    public PrimaryMeritCycleRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryMeritCycleRollup? Find(string code)
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

    public void Save(PrimaryMeritCycleRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryMeritCycleRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary merit cycle rollups. Empty fields match all.</summary>
public sealed record PrimaryMeritCycleRollupQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryMeritCycleRollup item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryMeritCycleRollup> Apply(List<PrimaryMeritCycleRollup> items)
    {
        var matched = Collections.Filter(items, (PrimaryMeritCycleRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary merit cycle rollup store.</summary>
public static class PrimaryMeritCycleRollupStore
{
    public static IPrimaryMeritCycleRollupStore NewMemoryPrimaryMeritCycleRollupStore()
    {
        return new MemoryPrimaryMeritCycleRollupStore(
            new Dictionary<string, PrimaryMeritCycleRollup>(),
            new List<string>());
    }

    public static IPrimaryMeritCycleRollupStore NewCachedPrimaryMeritCycleRollupStore(MemoryPrimaryMeritCycleRollupStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryMeritCycleRollupStore(
            inner,
            PrimaryMeritCycleRollupCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryMeritCycleRollup> FindPrimaryMeritCycleRollups(IPrimaryMeritCycleRollupStore store, PrimaryMeritCycleRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
