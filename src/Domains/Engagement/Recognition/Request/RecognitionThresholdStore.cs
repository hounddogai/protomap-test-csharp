namespace Acme.Workforce.Engagement.Recognition.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition thresholds by code.</summary>
public interface IRecognitionThresholdStore
{
    RecognitionThreshold? Find(string code);
    void Save(RecognitionThreshold item);
    bool Remove(string code);
    List<RecognitionThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionThresholdStore(Dictionary<string, RecognitionThreshold> items, List<string> order) : IRecognitionThresholdStore
{
    public Dictionary<string, RecognitionThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionThreshold item)
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

    public List<RecognitionThreshold> List()
    {
        var outValue = new List<RecognitionThreshold>();
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
public sealed class CachedRecognitionThresholdStore(MemoryRecognitionThresholdStore inner, RecognitionThresholdCache cache, int hits, int misses) : IRecognitionThresholdStore
{
    public MemoryRecognitionThresholdStore Inner { get; set; } = inner;
    public RecognitionThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionThreshold? Find(string code)
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

    public void Save(RecognitionThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition thresholds. Empty fields match all.</summary>
public sealed record RecognitionThresholdQuery(string Source, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionThreshold item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RecognitionThreshold> Apply(List<RecognitionThreshold> items)
    {
        var matched = Collections.Filter(items, (RecognitionThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition threshold store.</summary>
public static class RecognitionThresholdStore
{
    public static IRecognitionThresholdStore NewMemoryRecognitionThresholdStore()
    {
        return new MemoryRecognitionThresholdStore(
            new Dictionary<string, RecognitionThreshold>(),
            new List<string>());
    }

    public static IRecognitionThresholdStore NewCachedRecognitionThresholdStore(MemoryRecognitionThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionThresholdStore(inner, RecognitionThresholdCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RecognitionThreshold> FindRecognitionThresholds(IRecognitionThresholdStore store, RecognitionThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
