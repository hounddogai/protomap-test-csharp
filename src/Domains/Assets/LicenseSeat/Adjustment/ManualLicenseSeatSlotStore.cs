namespace Acme.Workforce.Assets.LicenseSeat.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual license seat slots by code.</summary>
public interface IManualLicenseSeatSlotStore
{
    ManualLicenseSeatSlot? Find(string code);
    void Save(ManualLicenseSeatSlot item);
    bool Remove(string code);
    List<ManualLicenseSeatSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualLicenseSeatSlotStore(Dictionary<string, ManualLicenseSeatSlot> items, List<string> order) : IManualLicenseSeatSlotStore
{
    public Dictionary<string, ManualLicenseSeatSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualLicenseSeatSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualLicenseSeatSlot item)
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

    public List<ManualLicenseSeatSlot> List()
    {
        var outValue = new List<ManualLicenseSeatSlot>();
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
public sealed class CachedManualLicenseSeatSlotStore(MemoryManualLicenseSeatSlotStore inner, ManualLicenseSeatSlotCache cache, int hits, int misses) : IManualLicenseSeatSlotStore
{
    public MemoryManualLicenseSeatSlotStore Inner { get; set; } = inner;
    public ManualLicenseSeatSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualLicenseSeatSlot? Find(string code)
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

    public void Save(ManualLicenseSeatSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualLicenseSeatSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual license seat slots. Empty fields match all.</summary>
public sealed record ManualLicenseSeatSlotQuery(string Region, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(ManualLicenseSeatSlot item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualLicenseSeatSlot> Apply(List<ManualLicenseSeatSlot> items)
    {
        var matched = Collections.Filter(items, (ManualLicenseSeatSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual license seat slot store.</summary>
public static class ManualLicenseSeatSlotStore
{
    public static IManualLicenseSeatSlotStore NewMemoryManualLicenseSeatSlotStore()
    {
        return new MemoryManualLicenseSeatSlotStore(
            new Dictionary<string, ManualLicenseSeatSlot>(),
            new List<string>());
    }

    public static IManualLicenseSeatSlotStore NewCachedManualLicenseSeatSlotStore(MemoryManualLicenseSeatSlotStore inner, int ttl, IClock clock)
    {
        return new CachedManualLicenseSeatSlotStore(inner, ManualLicenseSeatSlotCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ManualLicenseSeatSlot> FindManualLicenseSeatSlots(IManualLicenseSeatSlotStore store, ManualLicenseSeatSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
