namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal reorg snapshots by code.</summary>
public interface ISeasonalReorgSnapshotStore
{
    SeasonalReorgSnapshot? Find(string code);
    void Save(SeasonalReorgSnapshot item);
    bool Remove(string code);
    List<SeasonalReorgSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalReorgSnapshotStore(Dictionary<string, SeasonalReorgSnapshot> items, List<string> order) : ISeasonalReorgSnapshotStore
{
    public Dictionary<string, SeasonalReorgSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalReorgSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalReorgSnapshot item)
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

    public List<SeasonalReorgSnapshot> List()
    {
        var outValue = new List<SeasonalReorgSnapshot>();
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
public sealed class CachedSeasonalReorgSnapshotStore(MemorySeasonalReorgSnapshotStore inner, SeasonalReorgSnapshotCache cache, int hits, int misses) : ISeasonalReorgSnapshotStore
{
    public MemorySeasonalReorgSnapshotStore Inner { get; set; } = inner;
    public SeasonalReorgSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalReorgSnapshot? Find(string code)
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

    public void Save(SeasonalReorgSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalReorgSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal reorg snapshots. Empty fields match all.</summary>
public sealed record SeasonalReorgSnapshotQuery(string Label, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalReorgSnapshot item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<SeasonalReorgSnapshot> Apply(List<SeasonalReorgSnapshot> items)
    {
        var matched = Collections.Filter(items, (SeasonalReorgSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal reorg snapshot store.</summary>
public static class SeasonalReorgSnapshotStore
{
    public static ISeasonalReorgSnapshotStore NewMemorySeasonalReorgSnapshotStore()
    {
        return new MemorySeasonalReorgSnapshotStore(
            new Dictionary<string, SeasonalReorgSnapshot>(),
            new List<string>());
    }

    public static ISeasonalReorgSnapshotStore NewCachedSeasonalReorgSnapshotStore(MemorySeasonalReorgSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalReorgSnapshotStore(inner, SeasonalReorgSnapshotCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SeasonalReorgSnapshot> FindSeasonalReorgSnapshots(ISeasonalReorgSnapshotStore store, SeasonalReorgSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
