namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional bonus pool slots by code.</summary>
public interface IRegionalBonusPoolSlotStore
{
    RegionalBonusPoolSlot? Find(string code);
    void Save(RegionalBonusPoolSlot item);
    bool Remove(string code);
    List<RegionalBonusPoolSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalBonusPoolSlotStore(Dictionary<string, RegionalBonusPoolSlot> items, List<string> order) : IRegionalBonusPoolSlotStore
{
    public Dictionary<string, RegionalBonusPoolSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalBonusPoolSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalBonusPoolSlot item)
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

    public List<RegionalBonusPoolSlot> List()
    {
        var outValue = new List<RegionalBonusPoolSlot>();
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
public sealed class CachedRegionalBonusPoolSlotStore(MemoryRegionalBonusPoolSlotStore inner, RegionalBonusPoolSlotCache cache, int hits, int misses) : IRegionalBonusPoolSlotStore
{
    public MemoryRegionalBonusPoolSlotStore Inner { get; set; } = inner;
    public RegionalBonusPoolSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalBonusPoolSlot? Find(string code)
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

    public void Save(RegionalBonusPoolSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalBonusPoolSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional bonus pool slots. Empty fields match all.</summary>
public sealed record RegionalBonusPoolSlotQuery(string Locale, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalBonusPoolSlot item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalBonusPoolSlot> Apply(List<RegionalBonusPoolSlot> items)
    {
        var matched = Collections.Filter(items, (RegionalBonusPoolSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional bonus pool slot store.</summary>
public static class RegionalBonusPoolSlotStore
{
    public static IRegionalBonusPoolSlotStore NewMemoryRegionalBonusPoolSlotStore()
    {
        return new MemoryRegionalBonusPoolSlotStore(
            new Dictionary<string, RegionalBonusPoolSlot>(),
            new List<string>());
    }

    public static IRegionalBonusPoolSlotStore NewCachedRegionalBonusPoolSlotStore(MemoryRegionalBonusPoolSlotStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalBonusPoolSlotStore(inner, RegionalBonusPoolSlotCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RegionalBonusPoolSlot> FindRegionalBonusPoolSlots(IRegionalBonusPoolSlotStore store, RegionalBonusPoolSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
