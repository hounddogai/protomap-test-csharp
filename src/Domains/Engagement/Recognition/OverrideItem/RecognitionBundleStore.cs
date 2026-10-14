namespace Acme.Workforce.Engagement.Recognition.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition bundles by code.</summary>
public interface IRecognitionBundleStore
{
    RecognitionBundle? Find(string code);
    void Save(RecognitionBundle item);
    bool Remove(string code);
    List<RecognitionBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionBundleStore(Dictionary<string, RecognitionBundle> items, List<string> order) : IRecognitionBundleStore
{
    public Dictionary<string, RecognitionBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionBundle item)
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

    public List<RecognitionBundle> List()
    {
        var outValue = new List<RecognitionBundle>();
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
public sealed class CachedRecognitionBundleStore(MemoryRecognitionBundleStore inner, RecognitionBundleCache cache, int hits, int misses) : IRecognitionBundleStore
{
    public MemoryRecognitionBundleStore Inner { get; set; } = inner;
    public RecognitionBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionBundle? Find(string code)
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

    public void Save(RecognitionBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition bundles. Empty fields match all.</summary>
public sealed record RecognitionBundleQuery(string Country, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionBundle item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RecognitionBundle> Apply(List<RecognitionBundle> items)
    {
        var matched = Collections.Filter(items, (RecognitionBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition bundle store.</summary>
public static class RecognitionBundleStore
{
    public static IRecognitionBundleStore NewMemoryRecognitionBundleStore()
    {
        return new MemoryRecognitionBundleStore(new Dictionary<string, RecognitionBundle>(), new List<string>());
    }

    public static IRecognitionBundleStore NewCachedRecognitionBundleStore(MemoryRecognitionBundleStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionBundleStore(inner, RecognitionBundleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RecognitionBundle> FindRecognitionBundles(IRecognitionBundleStore store, RecognitionBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
