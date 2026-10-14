namespace Acme.Workforce.Engagement.Sentiment.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered sentiment targets by code.</summary>
public interface ITieredSentimentTargetStore
{
    TieredSentimentTarget? Find(string code);
    void Save(TieredSentimentTarget item);
    bool Remove(string code);
    List<TieredSentimentTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredSentimentTargetStore(Dictionary<string, TieredSentimentTarget> items, List<string> order) : ITieredSentimentTargetStore
{
    public Dictionary<string, TieredSentimentTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredSentimentTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredSentimentTarget item)
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

    public List<TieredSentimentTarget> List()
    {
        var outValue = new List<TieredSentimentTarget>();
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
public sealed class CachedTieredSentimentTargetStore(MemoryTieredSentimentTargetStore inner, TieredSentimentTargetCache cache, int hits, int misses) : ITieredSentimentTargetStore
{
    public MemoryTieredSentimentTargetStore Inner { get; set; } = inner;
    public TieredSentimentTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredSentimentTarget? Find(string code)
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

    public void Save(TieredSentimentTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredSentimentTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered sentiment targets. Empty fields match all.</summary>
public sealed record TieredSentimentTargetQuery(string Label, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(TieredSentimentTarget item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredSentimentTarget> Apply(List<TieredSentimentTarget> items)
    {
        var matched = Collections.Filter(items, (TieredSentimentTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered sentiment target store.</summary>
public static class TieredSentimentTargetStore
{
    public static ITieredSentimentTargetStore NewMemoryTieredSentimentTargetStore()
    {
        return new MemoryTieredSentimentTargetStore(
            new Dictionary<string, TieredSentimentTarget>(),
            new List<string>());
    }

    public static ITieredSentimentTargetStore NewCachedTieredSentimentTargetStore(MemoryTieredSentimentTargetStore inner, int ttl, IClock clock)
    {
        return new CachedTieredSentimentTargetStore(
            inner,
            TieredSentimentTargetCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<TieredSentimentTarget> FindTieredSentimentTargets(ITieredSentimentTargetStore store, TieredSentimentTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
