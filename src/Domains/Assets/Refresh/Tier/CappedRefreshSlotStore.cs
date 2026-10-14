namespace Acme.Workforce.Assets.Refresh.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped refresh slots by code.</summary>
public interface ICappedRefreshSlotStore
{
    CappedRefreshSlot? Find(string code);
    void Save(CappedRefreshSlot item);
    bool Remove(string code);
    List<CappedRefreshSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedRefreshSlotStore(Dictionary<string, CappedRefreshSlot> items, List<string> order) : ICappedRefreshSlotStore
{
    public Dictionary<string, CappedRefreshSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedRefreshSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedRefreshSlot item)
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

    public List<CappedRefreshSlot> List()
    {
        var outValue = new List<CappedRefreshSlot>();
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
public sealed class CachedCappedRefreshSlotStore(MemoryCappedRefreshSlotStore inner, CappedRefreshSlotCache cache, int hits, int misses) : ICappedRefreshSlotStore
{
    public MemoryCappedRefreshSlotStore Inner { get; set; } = inner;
    public CappedRefreshSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedRefreshSlot? Find(string code)
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

    public void Save(CappedRefreshSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedRefreshSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped refresh slots. Empty fields match all.</summary>
public sealed record CappedRefreshSlotQuery(string Label, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(CappedRefreshSlot item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedRefreshSlot> Apply(List<CappedRefreshSlot> items)
    {
        var matched = Collections.Filter(items, (CappedRefreshSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped refresh slot store.</summary>
public static class CappedRefreshSlotStore
{
    public static ICappedRefreshSlotStore NewMemoryCappedRefreshSlotStore()
    {
        return new MemoryCappedRefreshSlotStore(new Dictionary<string, CappedRefreshSlot>(), new List<string>());
    }

    public static ICappedRefreshSlotStore NewCachedCappedRefreshSlotStore(MemoryCappedRefreshSlotStore inner, int ttl, IClock clock)
    {
        return new CachedCappedRefreshSlotStore(inner, CappedRefreshSlotCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CappedRefreshSlot> FindCappedRefreshSlots(ICappedRefreshSlotStore store, CappedRefreshSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
