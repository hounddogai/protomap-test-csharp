namespace Acme.Workforce.Compensation.BonusPool.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient bonus pool slots by code.</summary>
public interface ILenientBonusPoolSlotStore
{
    LenientBonusPoolSlot? Find(string code);
    void Save(LenientBonusPoolSlot item);
    bool Remove(string code);
    List<LenientBonusPoolSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientBonusPoolSlotStore(Dictionary<string, LenientBonusPoolSlot> items, List<string> order) : ILenientBonusPoolSlotStore
{
    public Dictionary<string, LenientBonusPoolSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientBonusPoolSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientBonusPoolSlot item)
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

    public List<LenientBonusPoolSlot> List()
    {
        var outValue = new List<LenientBonusPoolSlot>();
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
public sealed class CachedLenientBonusPoolSlotStore(MemoryLenientBonusPoolSlotStore inner, LenientBonusPoolSlotCache cache, int hits, int misses) : ILenientBonusPoolSlotStore
{
    public MemoryLenientBonusPoolSlotStore Inner { get; set; } = inner;
    public LenientBonusPoolSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientBonusPoolSlot? Find(string code)
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

    public void Save(LenientBonusPoolSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientBonusPoolSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient bonus pool slots. Empty fields match all.</summary>
public sealed record LenientBonusPoolSlotQuery(string Department, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(LenientBonusPoolSlot item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientBonusPoolSlot> Apply(List<LenientBonusPoolSlot> items)
    {
        var matched = Collections.Filter(items, (LenientBonusPoolSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient bonus pool slot store.</summary>
public static class LenientBonusPoolSlotStore
{
    public static ILenientBonusPoolSlotStore NewMemoryLenientBonusPoolSlotStore()
    {
        return new MemoryLenientBonusPoolSlotStore(
            new Dictionary<string, LenientBonusPoolSlot>(),
            new List<string>());
    }

    public static ILenientBonusPoolSlotStore NewCachedLenientBonusPoolSlotStore(MemoryLenientBonusPoolSlotStore inner, int ttl, IClock clock)
    {
        return new CachedLenientBonusPoolSlotStore(inner, LenientBonusPoolSlotCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LenientBonusPoolSlot> FindLenientBonusPoolSlots(ILenientBonusPoolSlotStore store, LenientBonusPoolSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
