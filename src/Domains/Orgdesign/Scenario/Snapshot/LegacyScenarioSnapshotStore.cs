namespace Acme.Workforce.Orgdesign.Scenario.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy scenario snapshots by code.</summary>
public interface ILegacyScenarioSnapshotStore
{
    LegacyScenarioSnapshot? Find(string code);
    void Save(LegacyScenarioSnapshot item);
    bool Remove(string code);
    List<LegacyScenarioSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyScenarioSnapshotStore(Dictionary<string, LegacyScenarioSnapshot> items, List<string> order) : ILegacyScenarioSnapshotStore
{
    public Dictionary<string, LegacyScenarioSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyScenarioSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyScenarioSnapshot item)
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

    public List<LegacyScenarioSnapshot> List()
    {
        var outValue = new List<LegacyScenarioSnapshot>();
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
public sealed class CachedLegacyScenarioSnapshotStore(MemoryLegacyScenarioSnapshotStore inner, LegacyScenarioSnapshotCache cache, int hits, int misses) : ILegacyScenarioSnapshotStore
{
    public MemoryLegacyScenarioSnapshotStore Inner { get; set; } = inner;
    public LegacyScenarioSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyScenarioSnapshot? Find(string code)
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

    public void Save(LegacyScenarioSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyScenarioSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy scenario snapshots. Empty fields match all.</summary>
public sealed record LegacyScenarioSnapshotQuery(string Channel, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyScenarioSnapshot item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyScenarioSnapshot> Apply(List<LegacyScenarioSnapshot> items)
    {
        var matched = Collections.Filter(items, (LegacyScenarioSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy scenario snapshot store.</summary>
public static class LegacyScenarioSnapshotStore
{
    public static ILegacyScenarioSnapshotStore NewMemoryLegacyScenarioSnapshotStore()
    {
        return new MemoryLegacyScenarioSnapshotStore(
            new Dictionary<string, LegacyScenarioSnapshot>(),
            new List<string>());
    }

    public static ILegacyScenarioSnapshotStore NewCachedLegacyScenarioSnapshotStore(MemoryLegacyScenarioSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyScenarioSnapshotStore(
            inner,
            LegacyScenarioSnapshotCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LegacyScenarioSnapshot> FindLegacyScenarioSnapshots(ILegacyScenarioSnapshotStore store, LegacyScenarioSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
