namespace Acme.Workforce.Engagement.Recognition.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition allocations by code.</summary>
public interface IRecognitionAllocationStore
{
    RecognitionAllocation? Find(string code);
    void Save(RecognitionAllocation item);
    bool Remove(string code);
    List<RecognitionAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionAllocationStore(Dictionary<string, RecognitionAllocation> items, List<string> order) : IRecognitionAllocationStore
{
    public Dictionary<string, RecognitionAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionAllocation item)
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

    public List<RecognitionAllocation> List()
    {
        var outValue = new List<RecognitionAllocation>();
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
public sealed class CachedRecognitionAllocationStore(MemoryRecognitionAllocationStore inner, RecognitionAllocationCache cache, int hits, int misses) : IRecognitionAllocationStore
{
    public MemoryRecognitionAllocationStore Inner { get; set; } = inner;
    public RecognitionAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionAllocation? Find(string code)
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

    public void Save(RecognitionAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition allocations. Empty fields match all.</summary>
public sealed record RecognitionAllocationQuery(string Label, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionAllocation item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RecognitionAllocation> Apply(List<RecognitionAllocation> items)
    {
        var matched = Collections.Filter(items, (RecognitionAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition allocation store.</summary>
public static class RecognitionAllocationStore
{
    public static IRecognitionAllocationStore NewMemoryRecognitionAllocationStore()
    {
        return new MemoryRecognitionAllocationStore(
            new Dictionary<string, RecognitionAllocation>(),
            new List<string>());
    }

    public static IRecognitionAllocationStore NewCachedRecognitionAllocationStore(MemoryRecognitionAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionAllocationStore(inner, RecognitionAllocationCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RecognitionAllocation> FindRecognitionAllocations(IRecognitionAllocationStore store, RecognitionAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
