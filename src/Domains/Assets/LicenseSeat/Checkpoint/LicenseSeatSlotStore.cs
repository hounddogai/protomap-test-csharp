namespace Acme.Workforce.Assets.LicenseSeat.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps license seat slots by code.</summary>
public interface ILicenseSeatSlotStore
{
    LicenseSeatSlot? Find(string code);
    void Save(LicenseSeatSlot item);
    bool Remove(string code);
    List<LicenseSeatSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLicenseSeatSlotStore(Dictionary<string, LicenseSeatSlot> items, List<string> order) : ILicenseSeatSlotStore
{
    public Dictionary<string, LicenseSeatSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LicenseSeatSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LicenseSeatSlot item)
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

    public List<LicenseSeatSlot> List()
    {
        var outValue = new List<LicenseSeatSlot>();
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
public sealed class CachedLicenseSeatSlotStore(MemoryLicenseSeatSlotStore inner, LicenseSeatSlotCache cache, int hits, int misses) : ILicenseSeatSlotStore
{
    public MemoryLicenseSeatSlotStore Inner { get; set; } = inner;
    public LicenseSeatSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LicenseSeatSlot? Find(string code)
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

    public void Save(LicenseSeatSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LicenseSeatSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects license seat slots. Empty fields match all.</summary>
public sealed record LicenseSeatSlotQuery(string Name, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(LicenseSeatSlot item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LicenseSeatSlot> Apply(List<LicenseSeatSlot> items)
    {
        var matched = Collections.Filter(items, (LicenseSeatSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of license seat slot store.</summary>
public static class LicenseSeatSlotStore
{
    public static ILicenseSeatSlotStore NewMemoryLicenseSeatSlotStore()
    {
        return new MemoryLicenseSeatSlotStore(new Dictionary<string, LicenseSeatSlot>(), new List<string>());
    }

    public static ILicenseSeatSlotStore NewCachedLicenseSeatSlotStore(MemoryLicenseSeatSlotStore inner, int ttl, IClock clock)
    {
        return new CachedLicenseSeatSlotStore(inner, LicenseSeatSlotCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LicenseSeatSlot> FindLicenseSeatSlots(ILicenseSeatSlotStore store, LicenseSeatSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
