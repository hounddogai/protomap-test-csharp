namespace Acme.Workforce.Orgdesign.Span.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global span slots by code.</summary>
public interface IGlobalSpanSlotStore
{
    GlobalSpanSlot? Find(string code);
    void Save(GlobalSpanSlot item);
    bool Remove(string code);
    List<GlobalSpanSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalSpanSlotStore(Dictionary<string, GlobalSpanSlot> items, List<string> order) : IGlobalSpanSlotStore
{
    public Dictionary<string, GlobalSpanSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalSpanSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalSpanSlot item)
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

    public List<GlobalSpanSlot> List()
    {
        var outValue = new List<GlobalSpanSlot>();
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
public sealed class CachedGlobalSpanSlotStore(MemoryGlobalSpanSlotStore inner, GlobalSpanSlotCache cache, int hits, int misses) : IGlobalSpanSlotStore
{
    public MemoryGlobalSpanSlotStore Inner { get; set; } = inner;
    public GlobalSpanSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalSpanSlot? Find(string code)
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

    public void Save(GlobalSpanSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalSpanSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global span slots. Empty fields match all.</summary>
public sealed record GlobalSpanSlotQuery(string Channel, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalSpanSlot item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalSpanSlot> Apply(List<GlobalSpanSlot> items)
    {
        var matched = Collections.Filter(items, (GlobalSpanSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global span slot store.</summary>
public static class GlobalSpanSlotStore
{
    public static IGlobalSpanSlotStore NewMemoryGlobalSpanSlotStore()
    {
        return new MemoryGlobalSpanSlotStore(new Dictionary<string, GlobalSpanSlot>(), new List<string>());
    }

    public static IGlobalSpanSlotStore NewCachedGlobalSpanSlotStore(MemoryGlobalSpanSlotStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalSpanSlotStore(inner, GlobalSpanSlotCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<GlobalSpanSlot> FindGlobalSpanSlots(IGlobalSpanSlotStore store, GlobalSpanSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
