namespace Acme.Workforce.Engagement.PulseSurvey.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey snapshots by code.</summary>
public interface IPulseSurveySnapshotStore
{
    PulseSurveySnapshot? Find(string code);
    void Save(PulseSurveySnapshot item);
    bool Remove(string code);
    List<PulseSurveySnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveySnapshotStore(Dictionary<string, PulseSurveySnapshot> items, List<string> order) : IPulseSurveySnapshotStore
{
    public Dictionary<string, PulseSurveySnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveySnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveySnapshot item)
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

    public List<PulseSurveySnapshot> List()
    {
        var outValue = new List<PulseSurveySnapshot>();
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
public sealed class CachedPulseSurveySnapshotStore(MemoryPulseSurveySnapshotStore inner, PulseSurveySnapshotCache cache, int hits, int misses) : IPulseSurveySnapshotStore
{
    public MemoryPulseSurveySnapshotStore Inner { get; set; } = inner;
    public PulseSurveySnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveySnapshot? Find(string code)
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

    public void Save(PulseSurveySnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveySnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey snapshots. Empty fields match all.</summary>
public sealed record PulseSurveySnapshotQuery(string Title, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveySnapshot item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PulseSurveySnapshot> Apply(List<PulseSurveySnapshot> items)
    {
        var matched = Collections.Filter(items, (PulseSurveySnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey snapshot store.</summary>
public static class PulseSurveySnapshotStore
{
    public static IPulseSurveySnapshotStore NewMemoryPulseSurveySnapshotStore()
    {
        return new MemoryPulseSurveySnapshotStore(new Dictionary<string, PulseSurveySnapshot>(), new List<string>());
    }

    public static IPulseSurveySnapshotStore NewCachedPulseSurveySnapshotStore(MemoryPulseSurveySnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveySnapshotStore(inner, PulseSurveySnapshotCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PulseSurveySnapshot> FindPulseSurveySnapshots(IPulseSurveySnapshotStore store, PulseSurveySnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
