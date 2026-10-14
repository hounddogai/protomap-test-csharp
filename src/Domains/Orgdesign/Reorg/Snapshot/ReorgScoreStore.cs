namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg scores by code.</summary>
public interface IReorgScoreStore
{
    ReorgScore? Find(string code);
    void Save(ReorgScore item);
    bool Remove(string code);
    List<ReorgScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgScoreStore(Dictionary<string, ReorgScore> items, List<string> order) : IReorgScoreStore
{
    public Dictionary<string, ReorgScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgScore item)
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

    public List<ReorgScore> List()
    {
        var outValue = new List<ReorgScore>();
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
public sealed class CachedReorgScoreStore(MemoryReorgScoreStore inner, ReorgScoreCache cache, int hits, int misses) : IReorgScoreStore
{
    public MemoryReorgScoreStore Inner { get; set; } = inner;
    public ReorgScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgScore? Find(string code)
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

    public void Save(ReorgScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg scores. Empty fields match all.</summary>
public sealed record ReorgScoreQuery(string Channel, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgScore item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReorgScore> Apply(List<ReorgScore> items)
    {
        var matched = Collections.Filter(items, (ReorgScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg score store.</summary>
public static class ReorgScoreStore
{
    public static IReorgScoreStore NewMemoryReorgScoreStore()
    {
        return new MemoryReorgScoreStore(new Dictionary<string, ReorgScore>(), new List<string>());
    }

    public static IReorgScoreStore NewCachedReorgScoreStore(MemoryReorgScoreStore inner, int ttl, IClock clock)
    {
        return new CachedReorgScoreStore(inner, ReorgScoreCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ReorgScore> FindReorgScores(IReorgScoreStore store, ReorgScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
