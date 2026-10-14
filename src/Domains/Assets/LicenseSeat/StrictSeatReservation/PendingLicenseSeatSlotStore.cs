namespace Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending license seat slots by code.</summary>
public interface IPendingLicenseSeatSlotStore
{
    PendingLicenseSeatSlot? Find(string code);
    void Save(PendingLicenseSeatSlot item);
    bool Remove(string code);
    List<PendingLicenseSeatSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingLicenseSeatSlotStore(Dictionary<string, PendingLicenseSeatSlot> items, List<string> order) : IPendingLicenseSeatSlotStore
{
    public Dictionary<string, PendingLicenseSeatSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingLicenseSeatSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingLicenseSeatSlot item)
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

    public List<PendingLicenseSeatSlot> List()
    {
        var outValue = new List<PendingLicenseSeatSlot>();
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
public sealed class CachedPendingLicenseSeatSlotStore(MemoryPendingLicenseSeatSlotStore inner, PendingLicenseSeatSlotCache cache, int hits, int misses) : IPendingLicenseSeatSlotStore
{
    public MemoryPendingLicenseSeatSlotStore Inner { get; set; } = inner;
    public PendingLicenseSeatSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingLicenseSeatSlot? Find(string code)
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

    public void Save(PendingLicenseSeatSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingLicenseSeatSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending license seat slots. Empty fields match all.</summary>
public sealed record PendingLicenseSeatSlotQuery(string Category, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(PendingLicenseSeatSlot item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingLicenseSeatSlot> Apply(List<PendingLicenseSeatSlot> items)
    {
        var matched = Collections.Filter(items, (PendingLicenseSeatSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending license seat slot store.</summary>
public static class PendingLicenseSeatSlotStore
{
    public static IPendingLicenseSeatSlotStore NewMemoryPendingLicenseSeatSlotStore()
    {
        return new MemoryPendingLicenseSeatSlotStore(
            new Dictionary<string, PendingLicenseSeatSlot>(),
            new List<string>());
    }

    public static IPendingLicenseSeatSlotStore NewCachedPendingLicenseSeatSlotStore(MemoryPendingLicenseSeatSlotStore inner, int ttl, IClock clock)
    {
        return new CachedPendingLicenseSeatSlotStore(
            inner,
            PendingLicenseSeatSlotCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PendingLicenseSeatSlot> FindPendingLicenseSeatSlots(IPendingLicenseSeatSlotStore store, PendingLicenseSeatSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
