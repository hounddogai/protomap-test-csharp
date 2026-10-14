namespace Acme.Workforce.Contractors.Rate.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal rate snapshots by code.</summary>
public interface ISeasonalRateSnapshotStore
{
    SeasonalRateSnapshot? Find(string code);
    void Save(SeasonalRateSnapshot item);
    bool Remove(string code);
    List<SeasonalRateSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalRateSnapshotStore(Dictionary<string, SeasonalRateSnapshot> items, List<string> order) : ISeasonalRateSnapshotStore
{
    public Dictionary<string, SeasonalRateSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalRateSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalRateSnapshot item)
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

    public List<SeasonalRateSnapshot> List()
    {
        var outValue = new List<SeasonalRateSnapshot>();
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
public sealed class CachedSeasonalRateSnapshotStore(MemorySeasonalRateSnapshotStore inner, SeasonalRateSnapshotCache cache, int hits, int misses) : ISeasonalRateSnapshotStore
{
    public MemorySeasonalRateSnapshotStore Inner { get; set; } = inner;
    public SeasonalRateSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalRateSnapshot? Find(string code)
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

    public void Save(SeasonalRateSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalRateSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal rate snapshots. Empty fields match all.</summary>
public sealed record SeasonalRateSnapshotQuery(string Channel, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalRateSnapshot item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalRateSnapshot> Apply(List<SeasonalRateSnapshot> items)
    {
        var matched = Collections.Filter(items, (SeasonalRateSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal rate snapshot store.</summary>
public static class SeasonalRateSnapshotStore
{
    public static ISeasonalRateSnapshotStore NewMemorySeasonalRateSnapshotStore()
    {
        return new MemorySeasonalRateSnapshotStore(
            new Dictionary<string, SeasonalRateSnapshot>(),
            new List<string>());
    }

    public static ISeasonalRateSnapshotStore NewCachedSeasonalRateSnapshotStore(MemorySeasonalRateSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalRateSnapshotStore(inner, SeasonalRateSnapshotCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<SeasonalRateSnapshot> FindSeasonalRateSnapshots(ISeasonalRateSnapshotStore store, SeasonalRateSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
