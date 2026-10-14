namespace Acme.Workforce.Engagement.Recognition.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition entries by code.</summary>
public interface IRecognitionEntryStore
{
    RecognitionEntry? Find(string code);
    void Save(RecognitionEntry item);
    bool Remove(string code);
    List<RecognitionEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionEntryStore(Dictionary<string, RecognitionEntry> items, List<string> order) : IRecognitionEntryStore
{
    public Dictionary<string, RecognitionEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionEntry item)
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

    public List<RecognitionEntry> List()
    {
        var outValue = new List<RecognitionEntry>();
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
public sealed class CachedRecognitionEntryStore(MemoryRecognitionEntryStore inner, RecognitionEntryCache cache, int hits, int misses) : IRecognitionEntryStore
{
    public MemoryRecognitionEntryStore Inner { get; set; } = inner;
    public RecognitionEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionEntry? Find(string code)
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

    public void Save(RecognitionEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition entries. Empty fields match all.</summary>
public sealed record RecognitionEntryQuery(string Category, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionEntry item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<RecognitionEntry> Apply(List<RecognitionEntry> items)
    {
        var matched = Collections.Filter(items, (RecognitionEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition entry store.</summary>
public static class RecognitionEntryStore
{
    public static IRecognitionEntryStore NewMemoryRecognitionEntryStore()
    {
        return new MemoryRecognitionEntryStore(new Dictionary<string, RecognitionEntry>(), new List<string>());
    }

    public static IRecognitionEntryStore NewCachedRecognitionEntryStore(MemoryRecognitionEntryStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionEntryStore(inner, RecognitionEntryCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RecognitionEntry> FindRecognitionEntries(IRecognitionEntryStore store, RecognitionEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
