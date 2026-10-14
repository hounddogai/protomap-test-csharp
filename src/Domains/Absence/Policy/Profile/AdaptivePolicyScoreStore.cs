namespace Acme.Workforce.Absence.Policy.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive policy scores by code.</summary>
public interface IAdaptivePolicyScoreStore
{
    AdaptivePolicyScore? Find(string code);
    void Save(AdaptivePolicyScore item);
    bool Remove(string code);
    List<AdaptivePolicyScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptivePolicyScoreStore(Dictionary<string, AdaptivePolicyScore> items, List<string> order) : IAdaptivePolicyScoreStore
{
    public Dictionary<string, AdaptivePolicyScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptivePolicyScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptivePolicyScore item)
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

    public List<AdaptivePolicyScore> List()
    {
        var outValue = new List<AdaptivePolicyScore>();
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
public sealed class CachedAdaptivePolicyScoreStore(MemoryAdaptivePolicyScoreStore inner, AdaptivePolicyScoreCache cache, int hits, int misses) : IAdaptivePolicyScoreStore
{
    public MemoryAdaptivePolicyScoreStore Inner { get; set; } = inner;
    public AdaptivePolicyScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptivePolicyScore? Find(string code)
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

    public void Save(AdaptivePolicyScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptivePolicyScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive policy scores. Empty fields match all.</summary>
public sealed record AdaptivePolicyScoreQuery(string Title, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptivePolicyScore item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptivePolicyScore> Apply(List<AdaptivePolicyScore> items)
    {
        var matched = Collections.Filter(items, (AdaptivePolicyScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive policy score store.</summary>
public static class AdaptivePolicyScoreStore
{
    public static IAdaptivePolicyScoreStore NewMemoryAdaptivePolicyScoreStore()
    {
        return new MemoryAdaptivePolicyScoreStore(new Dictionary<string, AdaptivePolicyScore>(), new List<string>());
    }

    public static IAdaptivePolicyScoreStore NewCachedAdaptivePolicyScoreStore(MemoryAdaptivePolicyScoreStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptivePolicyScoreStore(inner, AdaptivePolicyScoreCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AdaptivePolicyScore> FindAdaptivePolicyScores(IAdaptivePolicyScoreStore store, AdaptivePolicyScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
