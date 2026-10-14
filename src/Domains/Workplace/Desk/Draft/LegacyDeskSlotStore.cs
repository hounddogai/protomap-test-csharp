namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy desk slots by code.</summary>
public interface ILegacyDeskSlotStore
{
    LegacyDeskSlot? Find(string code);
    void Save(LegacyDeskSlot item);
    bool Remove(string code);
    List<LegacyDeskSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyDeskSlotStore(Dictionary<string, LegacyDeskSlot> items, List<string> order) : ILegacyDeskSlotStore
{
    public Dictionary<string, LegacyDeskSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyDeskSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyDeskSlot item)
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

    public List<LegacyDeskSlot> List()
    {
        var outValue = new List<LegacyDeskSlot>();
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
public sealed class CachedLegacyDeskSlotStore(MemoryLegacyDeskSlotStore inner, LegacyDeskSlotCache cache, int hits, int misses) : ILegacyDeskSlotStore
{
    public MemoryLegacyDeskSlotStore Inner { get; set; } = inner;
    public LegacyDeskSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyDeskSlot? Find(string code)
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

    public void Save(LegacyDeskSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyDeskSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy desk slots. Empty fields match all.</summary>
public sealed record LegacyDeskSlotQuery(string Country, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyDeskSlot item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyDeskSlot> Apply(List<LegacyDeskSlot> items)
    {
        var matched = Collections.Filter(items, (LegacyDeskSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy desk slot store.</summary>
public static class LegacyDeskSlotStore
{
    public static ILegacyDeskSlotStore NewMemoryLegacyDeskSlotStore()
    {
        return new MemoryLegacyDeskSlotStore(new Dictionary<string, LegacyDeskSlot>(), new List<string>());
    }

    public static ILegacyDeskSlotStore NewCachedLegacyDeskSlotStore(MemoryLegacyDeskSlotStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyDeskSlotStore(inner, LegacyDeskSlotCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LegacyDeskSlot> FindLegacyDeskSlots(ILegacyDeskSlotStore store, LegacyDeskSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
