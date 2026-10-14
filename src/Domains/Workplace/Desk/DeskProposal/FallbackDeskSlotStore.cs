namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback desk slots by code.</summary>
public interface IFallbackDeskSlotStore
{
    FallbackDeskSlot? Find(string code);
    void Save(FallbackDeskSlot item);
    bool Remove(string code);
    List<FallbackDeskSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackDeskSlotStore(Dictionary<string, FallbackDeskSlot> items, List<string> order) : IFallbackDeskSlotStore
{
    public Dictionary<string, FallbackDeskSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackDeskSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackDeskSlot item)
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

    public List<FallbackDeskSlot> List()
    {
        var outValue = new List<FallbackDeskSlot>();
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
public sealed class CachedFallbackDeskSlotStore(MemoryFallbackDeskSlotStore inner, FallbackDeskSlotCache cache, int hits, int misses) : IFallbackDeskSlotStore
{
    public MemoryFallbackDeskSlotStore Inner { get; set; } = inner;
    public FallbackDeskSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackDeskSlot? Find(string code)
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

    public void Save(FallbackDeskSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackDeskSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback desk slots. Empty fields match all.</summary>
public sealed record FallbackDeskSlotQuery(string Department, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackDeskSlot item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackDeskSlot> Apply(List<FallbackDeskSlot> items)
    {
        var matched = Collections.Filter(items, (FallbackDeskSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback desk slot store.</summary>
public static class FallbackDeskSlotStore
{
    public static IFallbackDeskSlotStore NewMemoryFallbackDeskSlotStore()
    {
        return new MemoryFallbackDeskSlotStore(new Dictionary<string, FallbackDeskSlot>(), new List<string>());
    }

    public static IFallbackDeskSlotStore NewCachedFallbackDeskSlotStore(MemoryFallbackDeskSlotStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackDeskSlotStore(inner, FallbackDeskSlotCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<FallbackDeskSlot> FindFallbackDeskSlots(IFallbackDeskSlotStore store, FallbackDeskSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
