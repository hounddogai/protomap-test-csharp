namespace Acme.Workforce.Engagement.Sentiment.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global sentiment bundles by code.</summary>
public interface IGlobalSentimentBundleStore
{
    GlobalSentimentBundle? Find(string code);
    void Save(GlobalSentimentBundle item);
    bool Remove(string code);
    List<GlobalSentimentBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalSentimentBundleStore(Dictionary<string, GlobalSentimentBundle> items, List<string> order) : IGlobalSentimentBundleStore
{
    public Dictionary<string, GlobalSentimentBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalSentimentBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalSentimentBundle item)
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

    public List<GlobalSentimentBundle> List()
    {
        var outValue = new List<GlobalSentimentBundle>();
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
public sealed class CachedGlobalSentimentBundleStore(MemoryGlobalSentimentBundleStore inner, GlobalSentimentBundleCache cache, int hits, int misses) : IGlobalSentimentBundleStore
{
    public MemoryGlobalSentimentBundleStore Inner { get; set; } = inner;
    public GlobalSentimentBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalSentimentBundle? Find(string code)
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

    public void Save(GlobalSentimentBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalSentimentBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global sentiment bundles. Empty fields match all.</summary>
public sealed record GlobalSentimentBundleQuery(string Segment, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalSentimentBundle item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<GlobalSentimentBundle> Apply(List<GlobalSentimentBundle> items)
    {
        var matched = Collections.Filter(items, (GlobalSentimentBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global sentiment bundle store.</summary>
public static class GlobalSentimentBundleStore
{
    public static IGlobalSentimentBundleStore NewMemoryGlobalSentimentBundleStore()
    {
        return new MemoryGlobalSentimentBundleStore(
            new Dictionary<string, GlobalSentimentBundle>(),
            new List<string>());
    }

    public static IGlobalSentimentBundleStore NewCachedGlobalSentimentBundleStore(MemoryGlobalSentimentBundleStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalSentimentBundleStore(inner, GlobalSentimentBundleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<GlobalSentimentBundle> FindGlobalSentimentBundles(IGlobalSentimentBundleStore store, GlobalSentimentBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
