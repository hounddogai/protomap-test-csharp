namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount slots by code.</summary>
public interface IHeadcountSlotStore
{
    HeadcountSlot? Find(string code);
    void Save(HeadcountSlot item);
    bool Remove(string code);
    List<HeadcountSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountSlotStore(Dictionary<string, HeadcountSlot> items, List<string> order) : IHeadcountSlotStore
{
    public Dictionary<string, HeadcountSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountSlot item)
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

    public List<HeadcountSlot> List()
    {
        var outValue = new List<HeadcountSlot>();
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
public sealed class CachedHeadcountSlotStore(MemoryHeadcountSlotStore inner, HeadcountSlotCache cache, int hits, int misses) : IHeadcountSlotStore
{
    public MemoryHeadcountSlotStore Inner { get; set; } = inner;
    public HeadcountSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountSlot? Find(string code)
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

    public void Save(HeadcountSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount slots. Empty fields match all.</summary>
public sealed record HeadcountSlotQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountSlot item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountSlot> Apply(List<HeadcountSlot> items)
    {
        var matched = Collections.Filter(items, (HeadcountSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount slot store.</summary>
public static class HeadcountSlotStore
{
    public static IHeadcountSlotStore NewMemoryHeadcountSlotStore()
    {
        return new MemoryHeadcountSlotStore(new Dictionary<string, HeadcountSlot>(), new List<string>());
    }

    public static IHeadcountSlotStore NewCachedHeadcountSlotStore(MemoryHeadcountSlotStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountSlotStore(inner, HeadcountSlotCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<HeadcountSlot> FindHeadcountSlots(IHeadcountSlotStore store, HeadcountSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
