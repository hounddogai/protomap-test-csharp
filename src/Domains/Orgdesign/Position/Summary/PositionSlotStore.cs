namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position slots by code.</summary>
public interface IPositionSlotStore
{
    PositionSlot? Find(string code);
    void Save(PositionSlot item);
    bool Remove(string code);
    List<PositionSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionSlotStore(Dictionary<string, PositionSlot> items, List<string> order) : IPositionSlotStore
{
    public Dictionary<string, PositionSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionSlot item)
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

    public List<PositionSlot> List()
    {
        var outValue = new List<PositionSlot>();
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
public sealed class CachedPositionSlotStore(MemoryPositionSlotStore inner, PositionSlotCache cache, int hits, int misses) : IPositionSlotStore
{
    public MemoryPositionSlotStore Inner { get; set; } = inner;
    public PositionSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionSlot? Find(string code)
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

    public void Save(PositionSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position slots. Empty fields match all.</summary>
public sealed record PositionSlotQuery(string Channel, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(PositionSlot item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PositionSlot> Apply(List<PositionSlot> items)
    {
        var matched = Collections.Filter(items, (PositionSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position slot store.</summary>
public static class PositionSlotStore
{
    public static IPositionSlotStore NewMemoryPositionSlotStore()
    {
        return new MemoryPositionSlotStore(new Dictionary<string, PositionSlot>(), new List<string>());
    }

    public static IPositionSlotStore NewCachedPositionSlotStore(MemoryPositionSlotStore inner, int ttl, IClock clock)
    {
        return new CachedPositionSlotStore(inner, PositionSlotCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PositionSlot> FindPositionSlots(IPositionSlotStore store, PositionSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
