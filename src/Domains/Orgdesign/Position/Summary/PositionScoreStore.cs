namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position scores by code.</summary>
public interface IPositionScoreStore
{
    PositionScore? Find(string code);
    void Save(PositionScore item);
    bool Remove(string code);
    List<PositionScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionScoreStore(Dictionary<string, PositionScore> items, List<string> order) : IPositionScoreStore
{
    public Dictionary<string, PositionScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionScore item)
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

    public List<PositionScore> List()
    {
        var outValue = new List<PositionScore>();
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
public sealed class CachedPositionScoreStore(MemoryPositionScoreStore inner, PositionScoreCache cache, int hits, int misses) : IPositionScoreStore
{
    public MemoryPositionScoreStore Inner { get; set; } = inner;
    public PositionScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionScore? Find(string code)
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

    public void Save(PositionScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position scores. Empty fields match all.</summary>
public sealed record PositionScoreQuery(string Name, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(PositionScore item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PositionScore> Apply(List<PositionScore> items)
    {
        var matched = Collections.Filter(items, (PositionScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position score store.</summary>
public static class PositionScoreStore
{
    public static IPositionScoreStore NewMemoryPositionScoreStore()
    {
        return new MemoryPositionScoreStore(new Dictionary<string, PositionScore>(), new List<string>());
    }

    public static IPositionScoreStore NewCachedPositionScoreStore(MemoryPositionScoreStore inner, int ttl, IClock clock)
    {
        return new CachedPositionScoreStore(inner, PositionScoreCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PositionScore> FindPositionScores(IPositionScoreStore store, PositionScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
