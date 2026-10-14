namespace Acme.Workforce.Engagement.Recognition.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition quotas by code.</summary>
public interface IRecognitionQuotaStore
{
    RecognitionQuota? Find(string code);
    void Save(RecognitionQuota item);
    bool Remove(string code);
    List<RecognitionQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionQuotaStore(Dictionary<string, RecognitionQuota> items, List<string> order) : IRecognitionQuotaStore
{
    public Dictionary<string, RecognitionQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionQuota item)
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

    public List<RecognitionQuota> List()
    {
        var outValue = new List<RecognitionQuota>();
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
public sealed class CachedRecognitionQuotaStore(MemoryRecognitionQuotaStore inner, RecognitionQuotaCache cache, int hits, int misses) : IRecognitionQuotaStore
{
    public MemoryRecognitionQuotaStore Inner { get; set; } = inner;
    public RecognitionQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionQuota? Find(string code)
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

    public void Save(RecognitionQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition quotas. Empty fields match all.</summary>
public sealed record RecognitionQuotaQuery(string Title, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionQuota item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RecognitionQuota> Apply(List<RecognitionQuota> items)
    {
        var matched = Collections.Filter(items, (RecognitionQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition quota store.</summary>
public static class RecognitionQuotaStore
{
    public static IRecognitionQuotaStore NewMemoryRecognitionQuotaStore()
    {
        return new MemoryRecognitionQuotaStore(new Dictionary<string, RecognitionQuota>(), new List<string>());
    }

    public static IRecognitionQuotaStore NewCachedRecognitionQuotaStore(MemoryRecognitionQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionQuotaStore(inner, RecognitionQuotaCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RecognitionQuota> FindRecognitionQuotas(IRecognitionQuotaStore store, RecognitionQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
