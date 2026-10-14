namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback snapshots by code.</summary>
public interface IFeedbackSnapshotStore
{
    FeedbackSnapshot? Find(string code);
    void Save(FeedbackSnapshot item);
    bool Remove(string code);
    List<FeedbackSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackSnapshotStore(Dictionary<string, FeedbackSnapshot> items, List<string> order) : IFeedbackSnapshotStore
{
    public Dictionary<string, FeedbackSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackSnapshot item)
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

    public List<FeedbackSnapshot> List()
    {
        var outValue = new List<FeedbackSnapshot>();
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
public sealed class CachedFeedbackSnapshotStore(MemoryFeedbackSnapshotStore inner, FeedbackSnapshotCache cache, int hits, int misses) : IFeedbackSnapshotStore
{
    public MemoryFeedbackSnapshotStore Inner { get; set; } = inner;
    public FeedbackSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackSnapshot? Find(string code)
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

    public void Save(FeedbackSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback snapshots. Empty fields match all.</summary>
public sealed record FeedbackSnapshotQuery(string Team, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackSnapshot item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FeedbackSnapshot> Apply(List<FeedbackSnapshot> items)
    {
        var matched = Collections.Filter(items, (FeedbackSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback snapshot store.</summary>
public static class FeedbackSnapshotStore
{
    public static IFeedbackSnapshotStore NewMemoryFeedbackSnapshotStore()
    {
        return new MemoryFeedbackSnapshotStore(new Dictionary<string, FeedbackSnapshot>(), new List<string>());
    }

    public static IFeedbackSnapshotStore NewCachedFeedbackSnapshotStore(MemoryFeedbackSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackSnapshotStore(inner, FeedbackSnapshotCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<FeedbackSnapshot> FindFeedbackSnapshots(IFeedbackSnapshotStore store, FeedbackSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
