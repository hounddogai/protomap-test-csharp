namespace Acme.Workforce.Contractors.Engagement.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps engagement scores by code.</summary>
public interface IEngagementScoreStore
{
    EngagementScore? Find(string code);
    void Save(EngagementScore item);
    bool Remove(string code);
    List<EngagementScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEngagementScoreStore(Dictionary<string, EngagementScore> items, List<string> order) : IEngagementScoreStore
{
    public Dictionary<string, EngagementScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EngagementScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EngagementScore item)
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

    public List<EngagementScore> List()
    {
        var outValue = new List<EngagementScore>();
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
public sealed class CachedEngagementScoreStore(MemoryEngagementScoreStore inner, EngagementScoreCache cache, int hits, int misses) : IEngagementScoreStore
{
    public MemoryEngagementScoreStore Inner { get; set; } = inner;
    public EngagementScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EngagementScore? Find(string code)
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

    public void Save(EngagementScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EngagementScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects engagement scores. Empty fields match all.</summary>
public sealed record EngagementScoreQuery(string Label, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(EngagementScore item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<EngagementScore> Apply(List<EngagementScore> items)
    {
        var matched = Collections.Filter(items, (EngagementScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of engagement score store.</summary>
public static class EngagementScoreStore
{
    public static IEngagementScoreStore NewMemoryEngagementScoreStore()
    {
        return new MemoryEngagementScoreStore(new Dictionary<string, EngagementScore>(), new List<string>());
    }

    public static IEngagementScoreStore NewCachedEngagementScoreStore(MemoryEngagementScoreStore inner, int ttl, IClock clock)
    {
        return new CachedEngagementScoreStore(inner, EngagementScoreCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<EngagementScore> FindEngagementScores(IEngagementScoreStore store, EngagementScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
