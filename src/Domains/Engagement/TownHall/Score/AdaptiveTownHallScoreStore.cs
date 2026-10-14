namespace Acme.Workforce.Engagement.TownHall.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive town hall scores by code.</summary>
public interface IAdaptiveTownHallScoreStore
{
    AdaptiveTownHallScore? Find(string code);
    void Save(AdaptiveTownHallScore item);
    bool Remove(string code);
    List<AdaptiveTownHallScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveTownHallScoreStore(Dictionary<string, AdaptiveTownHallScore> items, List<string> order) : IAdaptiveTownHallScoreStore
{
    public Dictionary<string, AdaptiveTownHallScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveTownHallScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveTownHallScore item)
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

    public List<AdaptiveTownHallScore> List()
    {
        var outValue = new List<AdaptiveTownHallScore>();
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
public sealed class CachedAdaptiveTownHallScoreStore(MemoryAdaptiveTownHallScoreStore inner, AdaptiveTownHallScoreCache cache, int hits, int misses) : IAdaptiveTownHallScoreStore
{
    public MemoryAdaptiveTownHallScoreStore Inner { get; set; } = inner;
    public AdaptiveTownHallScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveTownHallScore? Find(string code)
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

    public void Save(AdaptiveTownHallScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveTownHallScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive town hall scores. Empty fields match all.</summary>
public sealed record AdaptiveTownHallScoreQuery(string Name, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveTownHallScore item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveTownHallScore> Apply(List<AdaptiveTownHallScore> items)
    {
        var matched = Collections.Filter(items, (AdaptiveTownHallScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive town hall score store.</summary>
public static class AdaptiveTownHallScoreStore
{
    public static IAdaptiveTownHallScoreStore NewMemoryAdaptiveTownHallScoreStore()
    {
        return new MemoryAdaptiveTownHallScoreStore(
            new Dictionary<string, AdaptiveTownHallScore>(),
            new List<string>());
    }

    public static IAdaptiveTownHallScoreStore NewCachedAdaptiveTownHallScoreStore(MemoryAdaptiveTownHallScoreStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveTownHallScoreStore(inner, AdaptiveTownHallScoreCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AdaptiveTownHallScore> FindAdaptiveTownHallScores(IAdaptiveTownHallScoreStore store, AdaptiveTownHallScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
