namespace Acme.Workforce.Orgdesign.Scenario.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional scenario batches by code.</summary>
public interface IRegionalScenarioBatchStore
{
    RegionalScenarioBatch? Find(string code);
    void Save(RegionalScenarioBatch item);
    bool Remove(string code);
    List<RegionalScenarioBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalScenarioBatchStore(Dictionary<string, RegionalScenarioBatch> items, List<string> order) : IRegionalScenarioBatchStore
{
    public Dictionary<string, RegionalScenarioBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalScenarioBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalScenarioBatch item)
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

    public List<RegionalScenarioBatch> List()
    {
        var outValue = new List<RegionalScenarioBatch>();
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
public sealed class CachedRegionalScenarioBatchStore(MemoryRegionalScenarioBatchStore inner, RegionalScenarioBatchCache cache, int hits, int misses) : IRegionalScenarioBatchStore
{
    public MemoryRegionalScenarioBatchStore Inner { get; set; } = inner;
    public RegionalScenarioBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalScenarioBatch? Find(string code)
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

    public void Save(RegionalScenarioBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalScenarioBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional scenario batches. Empty fields match all.</summary>
public sealed record RegionalScenarioBatchQuery(string Region, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalScenarioBatch item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalScenarioBatch> Apply(List<RegionalScenarioBatch> items)
    {
        var matched = Collections.Filter(items, (RegionalScenarioBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional scenario batch store.</summary>
public static class RegionalScenarioBatchStore
{
    public static IRegionalScenarioBatchStore NewMemoryRegionalScenarioBatchStore()
    {
        return new MemoryRegionalScenarioBatchStore(
            new Dictionary<string, RegionalScenarioBatch>(),
            new List<string>());
    }

    public static IRegionalScenarioBatchStore NewCachedRegionalScenarioBatchStore(MemoryRegionalScenarioBatchStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalScenarioBatchStore(inner, RegionalScenarioBatchCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RegionalScenarioBatch> FindRegionalScenarioBatches(IRegionalScenarioBatchStore store, RegionalScenarioBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
