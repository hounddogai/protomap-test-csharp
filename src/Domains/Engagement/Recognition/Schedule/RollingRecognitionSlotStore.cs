namespace Acme.Workforce.Engagement.Recognition.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling recognition slots by code.</summary>
public interface IRollingRecognitionSlotStore
{
    RollingRecognitionSlot? Find(string code);
    void Save(RollingRecognitionSlot item);
    bool Remove(string code);
    List<RollingRecognitionSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingRecognitionSlotStore(Dictionary<string, RollingRecognitionSlot> items, List<string> order) : IRollingRecognitionSlotStore
{
    public Dictionary<string, RollingRecognitionSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingRecognitionSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingRecognitionSlot item)
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

    public List<RollingRecognitionSlot> List()
    {
        var outValue = new List<RollingRecognitionSlot>();
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
public sealed class CachedRollingRecognitionSlotStore(MemoryRollingRecognitionSlotStore inner, RollingRecognitionSlotCache cache, int hits, int misses) : IRollingRecognitionSlotStore
{
    public MemoryRollingRecognitionSlotStore Inner { get; set; } = inner;
    public RollingRecognitionSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingRecognitionSlot? Find(string code)
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

    public void Save(RollingRecognitionSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingRecognitionSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling recognition slots. Empty fields match all.</summary>
public sealed record RollingRecognitionSlotQuery(string Country, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(RollingRecognitionSlot item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingRecognitionSlot> Apply(List<RollingRecognitionSlot> items)
    {
        var matched = Collections.Filter(items, (RollingRecognitionSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling recognition slot store.</summary>
public static class RollingRecognitionSlotStore
{
    public static IRollingRecognitionSlotStore NewMemoryRollingRecognitionSlotStore()
    {
        return new MemoryRollingRecognitionSlotStore(
            new Dictionary<string, RollingRecognitionSlot>(),
            new List<string>());
    }

    public static IRollingRecognitionSlotStore NewCachedRollingRecognitionSlotStore(MemoryRollingRecognitionSlotStore inner, int ttl, IClock clock)
    {
        return new CachedRollingRecognitionSlotStore(
            inner,
            RollingRecognitionSlotCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RollingRecognitionSlot> FindRollingRecognitionSlots(IRollingRecognitionSlotStore store, RollingRecognitionSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
