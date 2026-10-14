namespace Acme.Workforce.Engagement.Recognition.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition slots by code.</summary>
public interface IRecognitionSlotStore
{
    RecognitionSlot? Find(string code);
    void Save(RecognitionSlot item);
    bool Remove(string code);
    List<RecognitionSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionSlotStore(Dictionary<string, RecognitionSlot> items, List<string> order) : IRecognitionSlotStore
{
    public Dictionary<string, RecognitionSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionSlot item)
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

    public List<RecognitionSlot> List()
    {
        var outValue = new List<RecognitionSlot>();
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
public sealed class CachedRecognitionSlotStore(MemoryRecognitionSlotStore inner, RecognitionSlotCache cache, int hits, int misses) : IRecognitionSlotStore
{
    public MemoryRecognitionSlotStore Inner { get; set; } = inner;
    public RecognitionSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionSlot? Find(string code)
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

    public void Save(RecognitionSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition slots. Empty fields match all.</summary>
public sealed record RecognitionSlotQuery(string Channel, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionSlot item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RecognitionSlot> Apply(List<RecognitionSlot> items)
    {
        var matched = Collections.Filter(items, (RecognitionSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition slot store.</summary>
public static class RecognitionSlotStore
{
    public static IRecognitionSlotStore NewMemoryRecognitionSlotStore()
    {
        return new MemoryRecognitionSlotStore(new Dictionary<string, RecognitionSlot>(), new List<string>());
    }

    public static IRecognitionSlotStore NewCachedRecognitionSlotStore(MemoryRecognitionSlotStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionSlotStore(inner, RecognitionSlotCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RecognitionSlot> FindRecognitionSlots(IRecognitionSlotStore store, RecognitionSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
