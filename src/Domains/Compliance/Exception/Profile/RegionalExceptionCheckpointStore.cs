namespace Acme.Workforce.Compliance.Exception.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional exception checkpoints by code.</summary>
public interface IRegionalExceptionCheckpointStore
{
    RegionalExceptionCheckpoint? Find(string code);
    void Save(RegionalExceptionCheckpoint item);
    bool Remove(string code);
    List<RegionalExceptionCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalExceptionCheckpointStore(Dictionary<string, RegionalExceptionCheckpoint> items, List<string> order) : IRegionalExceptionCheckpointStore
{
    public Dictionary<string, RegionalExceptionCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalExceptionCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalExceptionCheckpoint item)
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

    public List<RegionalExceptionCheckpoint> List()
    {
        var outValue = new List<RegionalExceptionCheckpoint>();
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
public sealed class CachedRegionalExceptionCheckpointStore(MemoryRegionalExceptionCheckpointStore inner, RegionalExceptionCheckpointCache cache, int hits, int misses) : IRegionalExceptionCheckpointStore
{
    public MemoryRegionalExceptionCheckpointStore Inner { get; set; } = inner;
    public RegionalExceptionCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalExceptionCheckpoint? Find(string code)
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

    public void Save(RegionalExceptionCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalExceptionCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional exception checkpoints. Empty fields match all.</summary>
public sealed record RegionalExceptionCheckpointQuery(string Team, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalExceptionCheckpoint item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<RegionalExceptionCheckpoint> Apply(List<RegionalExceptionCheckpoint> items)
    {
        var matched = Collections.Filter(items, (RegionalExceptionCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional exception checkpoint store.</summary>
public static class RegionalExceptionCheckpointStore
{
    public static IRegionalExceptionCheckpointStore NewMemoryRegionalExceptionCheckpointStore()
    {
        return new MemoryRegionalExceptionCheckpointStore(
            new Dictionary<string, RegionalExceptionCheckpoint>(),
            new List<string>());
    }

    public static IRegionalExceptionCheckpointStore NewCachedRegionalExceptionCheckpointStore(MemoryRegionalExceptionCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalExceptionCheckpointStore(
            inner,
            RegionalExceptionCheckpointCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<RegionalExceptionCheckpoint> FindRegionalExceptionCheckpoints(IRegionalExceptionCheckpointStore store, RegionalExceptionCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
