namespace Acme.Workforce.Orgdesign.Span.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped span slots by code.</summary>
public interface ICappedSpanSlotStore
{
    CappedSpanSlot? Find(string code);
    void Save(CappedSpanSlot item);
    bool Remove(string code);
    List<CappedSpanSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedSpanSlotStore(Dictionary<string, CappedSpanSlot> items, List<string> order) : ICappedSpanSlotStore
{
    public Dictionary<string, CappedSpanSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedSpanSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedSpanSlot item)
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

    public List<CappedSpanSlot> List()
    {
        var outValue = new List<CappedSpanSlot>();
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
public sealed class CachedCappedSpanSlotStore(MemoryCappedSpanSlotStore inner, CappedSpanSlotCache cache, int hits, int misses) : ICappedSpanSlotStore
{
    public MemoryCappedSpanSlotStore Inner { get; set; } = inner;
    public CappedSpanSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedSpanSlot? Find(string code)
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

    public void Save(CappedSpanSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedSpanSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped span slots. Empty fields match all.</summary>
public sealed record CappedSpanSlotQuery(string Unit, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(CappedSpanSlot item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedSpanSlot> Apply(List<CappedSpanSlot> items)
    {
        var matched = Collections.Filter(items, (CappedSpanSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped span slot store.</summary>
public static class CappedSpanSlotStore
{
    public static ICappedSpanSlotStore NewMemoryCappedSpanSlotStore()
    {
        return new MemoryCappedSpanSlotStore(new Dictionary<string, CappedSpanSlot>(), new List<string>());
    }

    public static ICappedSpanSlotStore NewCachedCappedSpanSlotStore(MemoryCappedSpanSlotStore inner, int ttl, IClock clock)
    {
        return new CachedCappedSpanSlotStore(inner, CappedSpanSlotCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CappedSpanSlot> FindCappedSpanSlots(ICappedSpanSlotStore store, CappedSpanSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
