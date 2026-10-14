namespace Acme.Workforce.Orgdesign.Position.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global position rollups by code.</summary>
public interface IGlobalPositionRollupStore
{
    GlobalPositionRollup? Find(string code);
    void Save(GlobalPositionRollup item);
    bool Remove(string code);
    List<GlobalPositionRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalPositionRollupStore(Dictionary<string, GlobalPositionRollup> items, List<string> order) : IGlobalPositionRollupStore
{
    public Dictionary<string, GlobalPositionRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalPositionRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalPositionRollup item)
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

    public List<GlobalPositionRollup> List()
    {
        var outValue = new List<GlobalPositionRollup>();
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
public sealed class CachedGlobalPositionRollupStore(MemoryGlobalPositionRollupStore inner, GlobalPositionRollupCache cache, int hits, int misses) : IGlobalPositionRollupStore
{
    public MemoryGlobalPositionRollupStore Inner { get; set; } = inner;
    public GlobalPositionRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalPositionRollup? Find(string code)
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

    public void Save(GlobalPositionRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalPositionRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global position rollups. Empty fields match all.</summary>
public sealed record GlobalPositionRollupQuery(string Unit, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalPositionRollup item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalPositionRollup> Apply(List<GlobalPositionRollup> items)
    {
        var matched = Collections.Filter(items, (GlobalPositionRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global position rollup store.</summary>
public static class GlobalPositionRollupStore
{
    public static IGlobalPositionRollupStore NewMemoryGlobalPositionRollupStore()
    {
        return new MemoryGlobalPositionRollupStore(
            new Dictionary<string, GlobalPositionRollup>(),
            new List<string>());
    }

    public static IGlobalPositionRollupStore NewCachedGlobalPositionRollupStore(MemoryGlobalPositionRollupStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalPositionRollupStore(inner, GlobalPositionRollupCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<GlobalPositionRollup> FindGlobalPositionRollups(IGlobalPositionRollupStore store, GlobalPositionRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
