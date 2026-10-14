namespace Acme.Workforce.Engagement.Sentiment.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient sentiment exceptions by code.</summary>
public interface ILenientSentimentExceptionStore
{
    LenientSentimentException? Find(string code);
    void Save(LenientSentimentException item);
    bool Remove(string code);
    List<LenientSentimentException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientSentimentExceptionStore(Dictionary<string, LenientSentimentException> items, List<string> order) : ILenientSentimentExceptionStore
{
    public Dictionary<string, LenientSentimentException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientSentimentException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientSentimentException item)
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

    public List<LenientSentimentException> List()
    {
        var outValue = new List<LenientSentimentException>();
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
public sealed class CachedLenientSentimentExceptionStore(MemoryLenientSentimentExceptionStore inner, LenientSentimentExceptionCache cache, int hits, int misses) : ILenientSentimentExceptionStore
{
    public MemoryLenientSentimentExceptionStore Inner { get; set; } = inner;
    public LenientSentimentExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientSentimentException? Find(string code)
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

    public void Save(LenientSentimentException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientSentimentException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient sentiment exceptions. Empty fields match all.</summary>
public sealed record LenientSentimentExceptionQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(LenientSentimentException item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientSentimentException> Apply(List<LenientSentimentException> items)
    {
        var matched = Collections.Filter(items, (LenientSentimentException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient sentiment exception store.</summary>
public static class LenientSentimentExceptionStore
{
    public static ILenientSentimentExceptionStore NewMemoryLenientSentimentExceptionStore()
    {
        return new MemoryLenientSentimentExceptionStore(
            new Dictionary<string, LenientSentimentException>(),
            new List<string>());
    }

    public static ILenientSentimentExceptionStore NewCachedLenientSentimentExceptionStore(MemoryLenientSentimentExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedLenientSentimentExceptionStore(
            inner,
            LenientSentimentExceptionCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LenientSentimentException> FindLenientSentimentExceptions(ILenientSentimentExceptionStore store, LenientSentimentExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
