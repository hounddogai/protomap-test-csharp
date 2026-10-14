namespace Acme.Workforce.Compensation.Adjustment.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment slots by code.</summary>
public interface IAdjustmentSlotStore
{
    AdjustmentSlot? Find(string code);
    void Save(AdjustmentSlot item);
    bool Remove(string code);
    List<AdjustmentSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentSlotStore(Dictionary<string, AdjustmentSlot> items, List<string> order) : IAdjustmentSlotStore
{
    public Dictionary<string, AdjustmentSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentSlot item)
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

    public List<AdjustmentSlot> List()
    {
        var outValue = new List<AdjustmentSlot>();
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
public sealed class CachedAdjustmentSlotStore(MemoryAdjustmentSlotStore inner, AdjustmentSlotCache cache, int hits, int misses) : IAdjustmentSlotStore
{
    public MemoryAdjustmentSlotStore Inner { get; set; } = inner;
    public AdjustmentSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentSlot? Find(string code)
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

    public void Save(AdjustmentSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment slots. Empty fields match all.</summary>
public sealed record AdjustmentSlotQuery(string Title, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentSlot item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdjustmentSlot> Apply(List<AdjustmentSlot> items)
    {
        var matched = Collections.Filter(items, (AdjustmentSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment slot store.</summary>
public static class AdjustmentSlotStore
{
    public static IAdjustmentSlotStore NewMemoryAdjustmentSlotStore()
    {
        return new MemoryAdjustmentSlotStore(new Dictionary<string, AdjustmentSlot>(), new List<string>());
    }

    public static IAdjustmentSlotStore NewCachedAdjustmentSlotStore(MemoryAdjustmentSlotStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentSlotStore(inner, AdjustmentSlotCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AdjustmentSlot> FindAdjustmentSlots(IAdjustmentSlotStore store, AdjustmentSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
