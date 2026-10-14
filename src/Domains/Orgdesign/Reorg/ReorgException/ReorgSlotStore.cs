namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg slots by code.</summary>
public interface IReorgSlotStore
{
    ReorgSlot? Find(string code);
    void Save(ReorgSlot item);
    bool Remove(string code);
    List<ReorgSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgSlotStore(Dictionary<string, ReorgSlot> items, List<string> order) : IReorgSlotStore
{
    public Dictionary<string, ReorgSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgSlot item)
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

    public List<ReorgSlot> List()
    {
        var outValue = new List<ReorgSlot>();
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
public sealed class CachedReorgSlotStore(MemoryReorgSlotStore inner, ReorgSlotCache cache, int hits, int misses) : IReorgSlotStore
{
    public MemoryReorgSlotStore Inner { get; set; } = inner;
    public ReorgSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgSlot? Find(string code)
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

    public void Save(ReorgSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg slots. Empty fields match all.</summary>
public sealed record ReorgSlotQuery(string Segment, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgSlot item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReorgSlot> Apply(List<ReorgSlot> items)
    {
        var matched = Collections.Filter(items, (ReorgSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg slot store.</summary>
public static class ReorgSlotStore
{
    public static IReorgSlotStore NewMemoryReorgSlotStore()
    {
        return new MemoryReorgSlotStore(new Dictionary<string, ReorgSlot>(), new List<string>());
    }

    public static IReorgSlotStore NewCachedReorgSlotStore(MemoryReorgSlotStore inner, int ttl, IClock clock)
    {
        return new CachedReorgSlotStore(inner, ReorgSlotCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ReorgSlot> FindReorgSlots(IReorgSlotStore store, ReorgSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
