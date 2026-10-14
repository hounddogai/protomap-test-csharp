namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped recognition thresholds by code.</summary>
public interface ICappedRecognitionThresholdStore
{
    CappedRecognitionThreshold? Find(string code);
    void Save(CappedRecognitionThreshold item);
    bool Remove(string code);
    List<CappedRecognitionThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedRecognitionThresholdStore(Dictionary<string, CappedRecognitionThreshold> items, List<string> order) : ICappedRecognitionThresholdStore
{
    public Dictionary<string, CappedRecognitionThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedRecognitionThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedRecognitionThreshold item)
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

    public List<CappedRecognitionThreshold> List()
    {
        var outValue = new List<CappedRecognitionThreshold>();
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
public sealed class CachedCappedRecognitionThresholdStore(MemoryCappedRecognitionThresholdStore inner, CappedRecognitionThresholdCache cache, int hits, int misses) : ICappedRecognitionThresholdStore
{
    public MemoryCappedRecognitionThresholdStore Inner { get; set; } = inner;
    public CappedRecognitionThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedRecognitionThreshold? Find(string code)
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

    public void Save(CappedRecognitionThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedRecognitionThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped recognition thresholds. Empty fields match all.</summary>
public sealed record CappedRecognitionThresholdQuery(string Channel, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(CappedRecognitionThreshold item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedRecognitionThreshold> Apply(List<CappedRecognitionThreshold> items)
    {
        var matched = Collections.Filter(items, (CappedRecognitionThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped recognition threshold store.</summary>
public static class CappedRecognitionThresholdStore
{
    public static ICappedRecognitionThresholdStore NewMemoryCappedRecognitionThresholdStore()
    {
        return new MemoryCappedRecognitionThresholdStore(
            new Dictionary<string, CappedRecognitionThreshold>(),
            new List<string>());
    }

    public static ICappedRecognitionThresholdStore NewCachedCappedRecognitionThresholdStore(MemoryCappedRecognitionThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedCappedRecognitionThresholdStore(
            inner,
            CappedRecognitionThresholdCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<CappedRecognitionThreshold> FindCappedRecognitionThresholds(ICappedRecognitionThresholdStore store, CappedRecognitionThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
