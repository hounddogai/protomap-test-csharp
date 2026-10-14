namespace Acme.Workforce.Orgdesign.CostModel.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps cost model slots by code.</summary>
public interface ICostModelSlotStore
{
    CostModelSlot? Find(string code);
    void Save(CostModelSlot item);
    bool Remove(string code);
    List<CostModelSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCostModelSlotStore(Dictionary<string, CostModelSlot> items, List<string> order) : ICostModelSlotStore
{
    public Dictionary<string, CostModelSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CostModelSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CostModelSlot item)
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

    public List<CostModelSlot> List()
    {
        var outValue = new List<CostModelSlot>();
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
public sealed class CachedCostModelSlotStore(MemoryCostModelSlotStore inner, CostModelSlotCache cache, int hits, int misses) : ICostModelSlotStore
{
    public MemoryCostModelSlotStore Inner { get; set; } = inner;
    public CostModelSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CostModelSlot? Find(string code)
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

    public void Save(CostModelSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CostModelSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects cost model slots. Empty fields match all.</summary>
public sealed record CostModelSlotQuery(string Segment, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(CostModelSlot item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CostModelSlot> Apply(List<CostModelSlot> items)
    {
        var matched = Collections.Filter(items, (CostModelSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of cost model slot store.</summary>
public static class CostModelSlotStore
{
    public static ICostModelSlotStore NewMemoryCostModelSlotStore()
    {
        return new MemoryCostModelSlotStore(new Dictionary<string, CostModelSlot>(), new List<string>());
    }

    public static ICostModelSlotStore NewCachedCostModelSlotStore(MemoryCostModelSlotStore inner, int ttl, IClock clock)
    {
        return new CachedCostModelSlotStore(inner, CostModelSlotCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<CostModelSlot> FindCostModelSlots(ICostModelSlotStore store, CostModelSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
