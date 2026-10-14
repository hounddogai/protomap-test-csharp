namespace Acme.Workforce.Orgdesign.Span.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict span slots by code.</summary>
public interface IStrictSpanSlotStore
{
    StrictSpanSlot? Find(string code);
    void Save(StrictSpanSlot item);
    bool Remove(string code);
    List<StrictSpanSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictSpanSlotStore(Dictionary<string, StrictSpanSlot> items, List<string> order) : IStrictSpanSlotStore
{
    public Dictionary<string, StrictSpanSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictSpanSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictSpanSlot item)
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

    public List<StrictSpanSlot> List()
    {
        var outValue = new List<StrictSpanSlot>();
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
public sealed class CachedStrictSpanSlotStore(MemoryStrictSpanSlotStore inner, StrictSpanSlotCache cache, int hits, int misses) : IStrictSpanSlotStore
{
    public MemoryStrictSpanSlotStore Inner { get; set; } = inner;
    public StrictSpanSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictSpanSlot? Find(string code)
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

    public void Save(StrictSpanSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictSpanSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict span slots. Empty fields match all.</summary>
public sealed record StrictSpanSlotQuery(string Unit, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(StrictSpanSlot item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictSpanSlot> Apply(List<StrictSpanSlot> items)
    {
        var matched = Collections.Filter(items, (StrictSpanSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict span slot store.</summary>
public static class StrictSpanSlotStore
{
    public static IStrictSpanSlotStore NewMemoryStrictSpanSlotStore()
    {
        return new MemoryStrictSpanSlotStore(new Dictionary<string, StrictSpanSlot>(), new List<string>());
    }

    public static IStrictSpanSlotStore NewCachedStrictSpanSlotStore(MemoryStrictSpanSlotStore inner, int ttl, IClock clock)
    {
        return new CachedStrictSpanSlotStore(inner, StrictSpanSlotCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<StrictSpanSlot> FindStrictSpanSlots(IStrictSpanSlotStore store, StrictSpanSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
