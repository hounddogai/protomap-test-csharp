namespace Acme.Workforce.Workplace.Desk.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending desk rollups by code.</summary>
public interface IPendingDeskRollupStore
{
    PendingDeskRollup? Find(string code);
    void Save(PendingDeskRollup item);
    bool Remove(string code);
    List<PendingDeskRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingDeskRollupStore(Dictionary<string, PendingDeskRollup> items, List<string> order) : IPendingDeskRollupStore
{
    public Dictionary<string, PendingDeskRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingDeskRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingDeskRollup item)
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

    public List<PendingDeskRollup> List()
    {
        var outValue = new List<PendingDeskRollup>();
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
public sealed class CachedPendingDeskRollupStore(MemoryPendingDeskRollupStore inner, PendingDeskRollupCache cache, int hits, int misses) : IPendingDeskRollupStore
{
    public MemoryPendingDeskRollupStore Inner { get; set; } = inner;
    public PendingDeskRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingDeskRollup? Find(string code)
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

    public void Save(PendingDeskRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingDeskRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending desk rollups. Empty fields match all.</summary>
public sealed record PendingDeskRollupQuery(string Name, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(PendingDeskRollup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingDeskRollup> Apply(List<PendingDeskRollup> items)
    {
        var matched = Collections.Filter(items, (PendingDeskRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending desk rollup store.</summary>
public static class PendingDeskRollupStore
{
    public static IPendingDeskRollupStore NewMemoryPendingDeskRollupStore()
    {
        return new MemoryPendingDeskRollupStore(new Dictionary<string, PendingDeskRollup>(), new List<string>());
    }

    public static IPendingDeskRollupStore NewCachedPendingDeskRollupStore(MemoryPendingDeskRollupStore inner, int ttl, IClock clock)
    {
        return new CachedPendingDeskRollupStore(inner, PendingDeskRollupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PendingDeskRollup> FindPendingDeskRollups(IPendingDeskRollupStore store, PendingDeskRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
