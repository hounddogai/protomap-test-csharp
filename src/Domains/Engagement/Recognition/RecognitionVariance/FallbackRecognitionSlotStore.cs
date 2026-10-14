namespace Acme.Workforce.Engagement.Recognition.RecognitionVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback recognition slots by code.</summary>
public interface IFallbackRecognitionSlotStore
{
    FallbackRecognitionSlot? Find(string code);
    void Save(FallbackRecognitionSlot item);
    bool Remove(string code);
    List<FallbackRecognitionSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackRecognitionSlotStore(Dictionary<string, FallbackRecognitionSlot> items, List<string> order) : IFallbackRecognitionSlotStore
{
    public Dictionary<string, FallbackRecognitionSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackRecognitionSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackRecognitionSlot item)
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

    public List<FallbackRecognitionSlot> List()
    {
        var outValue = new List<FallbackRecognitionSlot>();
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
public sealed class CachedFallbackRecognitionSlotStore(MemoryFallbackRecognitionSlotStore inner, FallbackRecognitionSlotCache cache, int hits, int misses) : IFallbackRecognitionSlotStore
{
    public MemoryFallbackRecognitionSlotStore Inner { get; set; } = inner;
    public FallbackRecognitionSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackRecognitionSlot? Find(string code)
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

    public void Save(FallbackRecognitionSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackRecognitionSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback recognition slots. Empty fields match all.</summary>
public sealed record FallbackRecognitionSlotQuery(string Locale, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackRecognitionSlot item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackRecognitionSlot> Apply(List<FallbackRecognitionSlot> items)
    {
        var matched = Collections.Filter(items, (FallbackRecognitionSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback recognition slot store.</summary>
public static class FallbackRecognitionSlotStore
{
    public static IFallbackRecognitionSlotStore NewMemoryFallbackRecognitionSlotStore()
    {
        return new MemoryFallbackRecognitionSlotStore(
            new Dictionary<string, FallbackRecognitionSlot>(),
            new List<string>());
    }

    public static IFallbackRecognitionSlotStore NewCachedFallbackRecognitionSlotStore(MemoryFallbackRecognitionSlotStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackRecognitionSlotStore(
            inner,
            FallbackRecognitionSlotCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<FallbackRecognitionSlot> FindFallbackRecognitionSlots(IFallbackRecognitionSlotStore store, FallbackRecognitionSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
