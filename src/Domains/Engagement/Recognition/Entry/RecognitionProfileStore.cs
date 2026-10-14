namespace Acme.Workforce.Engagement.Recognition.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition profiles by code.</summary>
public interface IRecognitionProfileStore
{
    RecognitionProfile? Find(string code);
    void Save(RecognitionProfile item);
    bool Remove(string code);
    List<RecognitionProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionProfileStore(Dictionary<string, RecognitionProfile> items, List<string> order) : IRecognitionProfileStore
{
    public Dictionary<string, RecognitionProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionProfile item)
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

    public List<RecognitionProfile> List()
    {
        var outValue = new List<RecognitionProfile>();
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
public sealed class CachedRecognitionProfileStore(MemoryRecognitionProfileStore inner, RecognitionProfileCache cache, int hits, int misses) : IRecognitionProfileStore
{
    public MemoryRecognitionProfileStore Inner { get; set; } = inner;
    public RecognitionProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionProfile? Find(string code)
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

    public void Save(RecognitionProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition profiles. Empty fields match all.</summary>
public sealed record RecognitionProfileQuery(string Country, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionProfile item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RecognitionProfile> Apply(List<RecognitionProfile> items)
    {
        var matched = Collections.Filter(items, (RecognitionProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition profile store.</summary>
public static class RecognitionProfileStore
{
    public static IRecognitionProfileStore NewMemoryRecognitionProfileStore()
    {
        return new MemoryRecognitionProfileStore(new Dictionary<string, RecognitionProfile>(), new List<string>());
    }

    public static IRecognitionProfileStore NewCachedRecognitionProfileStore(MemoryRecognitionProfileStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionProfileStore(inner, RecognitionProfileCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RecognitionProfile> FindRecognitionProfiles(IRecognitionProfileStore store, RecognitionProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
