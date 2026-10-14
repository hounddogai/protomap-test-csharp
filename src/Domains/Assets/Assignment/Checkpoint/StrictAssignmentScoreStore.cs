namespace Acme.Workforce.Assets.Assignment.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict assignment scores by code.</summary>
public interface IStrictAssignmentScoreStore
{
    StrictAssignmentScore? Find(string code);
    void Save(StrictAssignmentScore item);
    bool Remove(string code);
    List<StrictAssignmentScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictAssignmentScoreStore(Dictionary<string, StrictAssignmentScore> items, List<string> order) : IStrictAssignmentScoreStore
{
    public Dictionary<string, StrictAssignmentScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictAssignmentScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictAssignmentScore item)
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

    public List<StrictAssignmentScore> List()
    {
        var outValue = new List<StrictAssignmentScore>();
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
public sealed class CachedStrictAssignmentScoreStore(MemoryStrictAssignmentScoreStore inner, StrictAssignmentScoreCache cache, int hits, int misses) : IStrictAssignmentScoreStore
{
    public MemoryStrictAssignmentScoreStore Inner { get; set; } = inner;
    public StrictAssignmentScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictAssignmentScore? Find(string code)
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

    public void Save(StrictAssignmentScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictAssignmentScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict assignment scores. Empty fields match all.</summary>
public sealed record StrictAssignmentScoreQuery(string Name, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(StrictAssignmentScore item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictAssignmentScore> Apply(List<StrictAssignmentScore> items)
    {
        var matched = Collections.Filter(items, (StrictAssignmentScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict assignment score store.</summary>
public static class StrictAssignmentScoreStore
{
    public static IStrictAssignmentScoreStore NewMemoryStrictAssignmentScoreStore()
    {
        return new MemoryStrictAssignmentScoreStore(
            new Dictionary<string, StrictAssignmentScore>(),
            new List<string>());
    }

    public static IStrictAssignmentScoreStore NewCachedStrictAssignmentScoreStore(MemoryStrictAssignmentScoreStore inner, int ttl, IClock clock)
    {
        return new CachedStrictAssignmentScoreStore(inner, StrictAssignmentScoreCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<StrictAssignmentScore> FindStrictAssignmentScores(IStrictAssignmentScoreStore store, StrictAssignmentScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
