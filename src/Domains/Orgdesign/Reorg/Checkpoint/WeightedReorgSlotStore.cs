namespace Acme.Workforce.Orgdesign.Reorg.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted reorg slots by code.</summary>
public interface IWeightedReorgSlotStore
{
    WeightedReorgSlot? Find(string code);
    void Save(WeightedReorgSlot item);
    bool Remove(string code);
    List<WeightedReorgSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedReorgSlotStore(Dictionary<string, WeightedReorgSlot> items, List<string> order) : IWeightedReorgSlotStore
{
    public Dictionary<string, WeightedReorgSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedReorgSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedReorgSlot item)
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

    public List<WeightedReorgSlot> List()
    {
        var outValue = new List<WeightedReorgSlot>();
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
public sealed class CachedWeightedReorgSlotStore(MemoryWeightedReorgSlotStore inner, WeightedReorgSlotCache cache, int hits, int misses) : IWeightedReorgSlotStore
{
    public MemoryWeightedReorgSlotStore Inner { get; set; } = inner;
    public WeightedReorgSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedReorgSlot? Find(string code)
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

    public void Save(WeightedReorgSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedReorgSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted reorg slots. Empty fields match all.</summary>
public sealed record WeightedReorgSlotQuery(string Label, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedReorgSlot item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedReorgSlot> Apply(List<WeightedReorgSlot> items)
    {
        var matched = Collections.Filter(items, (WeightedReorgSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted reorg slot store.</summary>
public static class WeightedReorgSlotStore
{
    public static IWeightedReorgSlotStore NewMemoryWeightedReorgSlotStore()
    {
        return new MemoryWeightedReorgSlotStore(new Dictionary<string, WeightedReorgSlot>(), new List<string>());
    }

    public static IWeightedReorgSlotStore NewCachedWeightedReorgSlotStore(MemoryWeightedReorgSlotStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedReorgSlotStore(inner, WeightedReorgSlotCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<WeightedReorgSlot> FindWeightedReorgSlots(IWeightedReorgSlotStore store, WeightedReorgSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
