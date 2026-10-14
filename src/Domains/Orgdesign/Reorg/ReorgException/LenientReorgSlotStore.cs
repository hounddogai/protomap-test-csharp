namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient reorg slots by code.</summary>
public interface ILenientReorgSlotStore
{
    LenientReorgSlot? Find(string code);
    void Save(LenientReorgSlot item);
    bool Remove(string code);
    List<LenientReorgSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientReorgSlotStore(Dictionary<string, LenientReorgSlot> items, List<string> order) : ILenientReorgSlotStore
{
    public Dictionary<string, LenientReorgSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientReorgSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientReorgSlot item)
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

    public List<LenientReorgSlot> List()
    {
        var outValue = new List<LenientReorgSlot>();
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
public sealed class CachedLenientReorgSlotStore(MemoryLenientReorgSlotStore inner, LenientReorgSlotCache cache, int hits, int misses) : ILenientReorgSlotStore
{
    public MemoryLenientReorgSlotStore Inner { get; set; } = inner;
    public LenientReorgSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientReorgSlot? Find(string code)
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

    public void Save(LenientReorgSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientReorgSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient reorg slots. Empty fields match all.</summary>
public sealed record LenientReorgSlotQuery(string Label, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(LenientReorgSlot item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<LenientReorgSlot> Apply(List<LenientReorgSlot> items)
    {
        var matched = Collections.Filter(items, (LenientReorgSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient reorg slot store.</summary>
public static class LenientReorgSlotStore
{
    public static ILenientReorgSlotStore NewMemoryLenientReorgSlotStore()
    {
        return new MemoryLenientReorgSlotStore(new Dictionary<string, LenientReorgSlot>(), new List<string>());
    }

    public static ILenientReorgSlotStore NewCachedLenientReorgSlotStore(MemoryLenientReorgSlotStore inner, int ttl, IClock clock)
    {
        return new CachedLenientReorgSlotStore(inner, LenientReorgSlotCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LenientReorgSlot> FindLenientReorgSlots(ILenientReorgSlotStore store, LenientReorgSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
