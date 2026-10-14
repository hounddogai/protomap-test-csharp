namespace Acme.Workforce.Contractors.Rate.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling rate slots by code.</summary>
public interface IRollingRateSlotStore
{
    RollingRateSlot? Find(string code);
    void Save(RollingRateSlot item);
    bool Remove(string code);
    List<RollingRateSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingRateSlotStore(Dictionary<string, RollingRateSlot> items, List<string> order) : IRollingRateSlotStore
{
    public Dictionary<string, RollingRateSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingRateSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingRateSlot item)
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

    public List<RollingRateSlot> List()
    {
        var outValue = new List<RollingRateSlot>();
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
public sealed class CachedRollingRateSlotStore(MemoryRollingRateSlotStore inner, RollingRateSlotCache cache, int hits, int misses) : IRollingRateSlotStore
{
    public MemoryRollingRateSlotStore Inner { get; set; } = inner;
    public RollingRateSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingRateSlot? Find(string code)
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

    public void Save(RollingRateSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingRateSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling rate slots. Empty fields match all.</summary>
public sealed record RollingRateSlotQuery(string Label, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(RollingRateSlot item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<RollingRateSlot> Apply(List<RollingRateSlot> items)
    {
        var matched = Collections.Filter(items, (RollingRateSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling rate slot store.</summary>
public static class RollingRateSlotStore
{
    public static IRollingRateSlotStore NewMemoryRollingRateSlotStore()
    {
        return new MemoryRollingRateSlotStore(new Dictionary<string, RollingRateSlot>(), new List<string>());
    }

    public static IRollingRateSlotStore NewCachedRollingRateSlotStore(MemoryRollingRateSlotStore inner, int ttl, IClock clock)
    {
        return new CachedRollingRateSlotStore(inner, RollingRateSlotCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RollingRateSlot> FindRollingRateSlots(IRollingRateSlotStore store, RollingRateSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
