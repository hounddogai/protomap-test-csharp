namespace Acme.Workforce.Contractors.Engagement.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global engagement scores by code.</summary>
public interface IGlobalEngagementScoreStore
{
    GlobalEngagementScore? Find(string code);
    void Save(GlobalEngagementScore item);
    bool Remove(string code);
    List<GlobalEngagementScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalEngagementScoreStore(Dictionary<string, GlobalEngagementScore> items, List<string> order) : IGlobalEngagementScoreStore
{
    public Dictionary<string, GlobalEngagementScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalEngagementScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalEngagementScore item)
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

    public List<GlobalEngagementScore> List()
    {
        var outValue = new List<GlobalEngagementScore>();
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
public sealed class CachedGlobalEngagementScoreStore(MemoryGlobalEngagementScoreStore inner, GlobalEngagementScoreCache cache, int hits, int misses) : IGlobalEngagementScoreStore
{
    public MemoryGlobalEngagementScoreStore Inner { get; set; } = inner;
    public GlobalEngagementScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalEngagementScore? Find(string code)
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

    public void Save(GlobalEngagementScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalEngagementScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global engagement scores. Empty fields match all.</summary>
public sealed record GlobalEngagementScoreQuery(string Title, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalEngagementScore item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalEngagementScore> Apply(List<GlobalEngagementScore> items)
    {
        var matched = Collections.Filter(items, (GlobalEngagementScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global engagement score store.</summary>
public static class GlobalEngagementScoreStore
{
    public static IGlobalEngagementScoreStore NewMemoryGlobalEngagementScoreStore()
    {
        return new MemoryGlobalEngagementScoreStore(
            new Dictionary<string, GlobalEngagementScore>(),
            new List<string>());
    }

    public static IGlobalEngagementScoreStore NewCachedGlobalEngagementScoreStore(MemoryGlobalEngagementScoreStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalEngagementScoreStore(
            inner,
            GlobalEngagementScoreCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<GlobalEngagementScore> FindGlobalEngagementScores(IGlobalEngagementScoreStore store, GlobalEngagementScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
