namespace Acme.Workforce.Compensation.EquityGrant.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant slots by code.</summary>
public interface IEquityGrantSlotStore
{
    EquityGrantSlot? Find(string code);
    void Save(EquityGrantSlot item);
    bool Remove(string code);
    List<EquityGrantSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantSlotStore(Dictionary<string, EquityGrantSlot> items, List<string> order) : IEquityGrantSlotStore
{
    public Dictionary<string, EquityGrantSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantSlot item)
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

    public List<EquityGrantSlot> List()
    {
        var outValue = new List<EquityGrantSlot>();
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
public sealed class CachedEquityGrantSlotStore(MemoryEquityGrantSlotStore inner, EquityGrantSlotCache cache, int hits, int misses) : IEquityGrantSlotStore
{
    public MemoryEquityGrantSlotStore Inner { get; set; } = inner;
    public EquityGrantSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantSlot? Find(string code)
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

    public void Save(EquityGrantSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant slots. Empty fields match all.</summary>
public sealed record EquityGrantSlotQuery(string Locale, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantSlot item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantSlot> Apply(List<EquityGrantSlot> items)
    {
        var matched = Collections.Filter(items, (EquityGrantSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant slot store.</summary>
public static class EquityGrantSlotStore
{
    public static IEquityGrantSlotStore NewMemoryEquityGrantSlotStore()
    {
        return new MemoryEquityGrantSlotStore(new Dictionary<string, EquityGrantSlot>(), new List<string>());
    }

    public static IEquityGrantSlotStore NewCachedEquityGrantSlotStore(MemoryEquityGrantSlotStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantSlotStore(inner, EquityGrantSlotCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<EquityGrantSlot> FindEquityGrantSlots(IEquityGrantSlotStore store, EquityGrantSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
