namespace Acme.Workforce.Talent.Pool.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending pool slots by code.</summary>
public interface IPendingPoolSlotStore
{
    PendingPoolSlot? Find(string code);
    void Save(PendingPoolSlot item);
    bool Remove(string code);
    List<PendingPoolSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingPoolSlotStore(Dictionary<string, PendingPoolSlot> items, List<string> order) : IPendingPoolSlotStore
{
    public Dictionary<string, PendingPoolSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingPoolSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingPoolSlot item)
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

    public List<PendingPoolSlot> List()
    {
        var outValue = new List<PendingPoolSlot>();
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
public sealed class CachedPendingPoolSlotStore(MemoryPendingPoolSlotStore inner, PendingPoolSlotCache cache, int hits, int misses) : IPendingPoolSlotStore
{
    public MemoryPendingPoolSlotStore Inner { get; set; } = inner;
    public PendingPoolSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingPoolSlot? Find(string code)
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

    public void Save(PendingPoolSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingPoolSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending pool slots. Empty fields match all.</summary>
public sealed record PendingPoolSlotQuery(string Channel, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(PendingPoolSlot item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingPoolSlot> Apply(List<PendingPoolSlot> items)
    {
        var matched = Collections.Filter(items, (PendingPoolSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending pool slot store.</summary>
public static class PendingPoolSlotStore
{
    public static IPendingPoolSlotStore NewMemoryPendingPoolSlotStore()
    {
        return new MemoryPendingPoolSlotStore(new Dictionary<string, PendingPoolSlot>(), new List<string>());
    }

    public static IPendingPoolSlotStore NewCachedPendingPoolSlotStore(MemoryPendingPoolSlotStore inner, int ttl, IClock clock)
    {
        return new CachedPendingPoolSlotStore(inner, PendingPoolSlotCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PendingPoolSlot> FindPendingPoolSlots(IPendingPoolSlotStore store, PendingPoolSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
