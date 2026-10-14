namespace Acme.Workforce.Engagement.PulseSurvey.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary pulse survey snapshots by code.</summary>
public interface IPrimaryPulseSurveySnapshotStore
{
    PrimaryPulseSurveySnapshot? Find(string code);
    void Save(PrimaryPulseSurveySnapshot item);
    bool Remove(string code);
    List<PrimaryPulseSurveySnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryPulseSurveySnapshotStore(Dictionary<string, PrimaryPulseSurveySnapshot> items, List<string> order) : IPrimaryPulseSurveySnapshotStore
{
    public Dictionary<string, PrimaryPulseSurveySnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryPulseSurveySnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryPulseSurveySnapshot item)
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

    public List<PrimaryPulseSurveySnapshot> List()
    {
        var outValue = new List<PrimaryPulseSurveySnapshot>();
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
public sealed class CachedPrimaryPulseSurveySnapshotStore(MemoryPrimaryPulseSurveySnapshotStore inner, PrimaryPulseSurveySnapshotCache cache, int hits, int misses) : IPrimaryPulseSurveySnapshotStore
{
    public MemoryPrimaryPulseSurveySnapshotStore Inner { get; set; } = inner;
    public PrimaryPulseSurveySnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryPulseSurveySnapshot? Find(string code)
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

    public void Save(PrimaryPulseSurveySnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryPulseSurveySnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary pulse survey snapshots. Empty fields match all.</summary>
public sealed record PrimaryPulseSurveySnapshotQuery(string Name, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryPulseSurveySnapshot item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<PrimaryPulseSurveySnapshot> Apply(List<PrimaryPulseSurveySnapshot> items)
    {
        var matched = Collections.Filter(items, (PrimaryPulseSurveySnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary pulse survey snapshot store.</summary>
public static class PrimaryPulseSurveySnapshotStore
{
    public static IPrimaryPulseSurveySnapshotStore NewMemoryPrimaryPulseSurveySnapshotStore()
    {
        return new MemoryPrimaryPulseSurveySnapshotStore(
            new Dictionary<string, PrimaryPulseSurveySnapshot>(),
            new List<string>());
    }

    public static IPrimaryPulseSurveySnapshotStore NewCachedPrimaryPulseSurveySnapshotStore(MemoryPrimaryPulseSurveySnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryPulseSurveySnapshotStore(
            inner,
            PrimaryPulseSurveySnapshotCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryPulseSurveySnapshot> FindPrimaryPulseSurveySnapshots(IPrimaryPulseSurveySnapshotStore store, PrimaryPulseSurveySnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
