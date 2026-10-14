namespace Acme.Workforce.Talent.Nomination.NominationPolicy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled nomination snapshots by code.</summary>
public interface IScheduledNominationSnapshotStore
{
    ScheduledNominationSnapshot? Find(string code);
    void Save(ScheduledNominationSnapshot item);
    bool Remove(string code);
    List<ScheduledNominationSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledNominationSnapshotStore(Dictionary<string, ScheduledNominationSnapshot> items, List<string> order) : IScheduledNominationSnapshotStore
{
    public Dictionary<string, ScheduledNominationSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledNominationSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledNominationSnapshot item)
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

    public List<ScheduledNominationSnapshot> List()
    {
        var outValue = new List<ScheduledNominationSnapshot>();
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
public sealed class CachedScheduledNominationSnapshotStore(MemoryScheduledNominationSnapshotStore inner, ScheduledNominationSnapshotCache cache, int hits, int misses) : IScheduledNominationSnapshotStore
{
    public MemoryScheduledNominationSnapshotStore Inner { get; set; } = inner;
    public ScheduledNominationSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledNominationSnapshot? Find(string code)
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

    public void Save(ScheduledNominationSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledNominationSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled nomination snapshots. Empty fields match all.</summary>
public sealed record ScheduledNominationSnapshotQuery(string Locale, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledNominationSnapshot item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledNominationSnapshot> Apply(List<ScheduledNominationSnapshot> items)
    {
        var matched = Collections.Filter(items, (ScheduledNominationSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled nomination snapshot store.</summary>
public static class ScheduledNominationSnapshotStore
{
    public static IScheduledNominationSnapshotStore NewMemoryScheduledNominationSnapshotStore()
    {
        return new MemoryScheduledNominationSnapshotStore(
            new Dictionary<string, ScheduledNominationSnapshot>(),
            new List<string>());
    }

    public static IScheduledNominationSnapshotStore NewCachedScheduledNominationSnapshotStore(MemoryScheduledNominationSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledNominationSnapshotStore(
            inner,
            ScheduledNominationSnapshotCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledNominationSnapshot> FindScheduledNominationSnapshots(IScheduledNominationSnapshotStore store, ScheduledNominationSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
