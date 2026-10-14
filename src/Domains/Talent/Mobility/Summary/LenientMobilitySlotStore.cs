namespace Acme.Workforce.Talent.Mobility.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient mobility slots by code.</summary>
public interface ILenientMobilitySlotStore
{
    LenientMobilitySlot? Find(string code);
    void Save(LenientMobilitySlot item);
    bool Remove(string code);
    List<LenientMobilitySlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientMobilitySlotStore(Dictionary<string, LenientMobilitySlot> items, List<string> order) : ILenientMobilitySlotStore
{
    public Dictionary<string, LenientMobilitySlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientMobilitySlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientMobilitySlot item)
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

    public List<LenientMobilitySlot> List()
    {
        var outValue = new List<LenientMobilitySlot>();
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
public sealed class CachedLenientMobilitySlotStore(MemoryLenientMobilitySlotStore inner, LenientMobilitySlotCache cache, int hits, int misses) : ILenientMobilitySlotStore
{
    public MemoryLenientMobilitySlotStore Inner { get; set; } = inner;
    public LenientMobilitySlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientMobilitySlot? Find(string code)
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

    public void Save(LenientMobilitySlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientMobilitySlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient mobility slots. Empty fields match all.</summary>
public sealed record LenientMobilitySlotQuery(string Department, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(LenientMobilitySlot item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientMobilitySlot> Apply(List<LenientMobilitySlot> items)
    {
        var matched = Collections.Filter(items, (LenientMobilitySlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient mobility slot store.</summary>
public static class LenientMobilitySlotStore
{
    public static ILenientMobilitySlotStore NewMemoryLenientMobilitySlotStore()
    {
        return new MemoryLenientMobilitySlotStore(new Dictionary<string, LenientMobilitySlot>(), new List<string>());
    }

    public static ILenientMobilitySlotStore NewCachedLenientMobilitySlotStore(MemoryLenientMobilitySlotStore inner, int ttl, IClock clock)
    {
        return new CachedLenientMobilitySlotStore(inner, LenientMobilitySlotCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LenientMobilitySlot> FindLenientMobilitySlots(ILenientMobilitySlotStore store, LenientMobilitySlotQuery query)
    {
        return query.Apply(store.List());
    }
}
