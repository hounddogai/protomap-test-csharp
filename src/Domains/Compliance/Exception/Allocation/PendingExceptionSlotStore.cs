namespace Acme.Workforce.Compliance.Exception.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending exception slots by code.</summary>
public interface IPendingExceptionSlotStore
{
    PendingExceptionSlot? Find(string code);
    void Save(PendingExceptionSlot item);
    bool Remove(string code);
    List<PendingExceptionSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingExceptionSlotStore(Dictionary<string, PendingExceptionSlot> items, List<string> order) : IPendingExceptionSlotStore
{
    public Dictionary<string, PendingExceptionSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingExceptionSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingExceptionSlot item)
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

    public List<PendingExceptionSlot> List()
    {
        var outValue = new List<PendingExceptionSlot>();
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
public sealed class CachedPendingExceptionSlotStore(MemoryPendingExceptionSlotStore inner, PendingExceptionSlotCache cache, int hits, int misses) : IPendingExceptionSlotStore
{
    public MemoryPendingExceptionSlotStore Inner { get; set; } = inner;
    public PendingExceptionSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingExceptionSlot? Find(string code)
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

    public void Save(PendingExceptionSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingExceptionSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending exception slots. Empty fields match all.</summary>
public sealed record PendingExceptionSlotQuery(string Team, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(PendingExceptionSlot item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<PendingExceptionSlot> Apply(List<PendingExceptionSlot> items)
    {
        var matched = Collections.Filter(items, (PendingExceptionSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending exception slot store.</summary>
public static class PendingExceptionSlotStore
{
    public static IPendingExceptionSlotStore NewMemoryPendingExceptionSlotStore()
    {
        return new MemoryPendingExceptionSlotStore(
            new Dictionary<string, PendingExceptionSlot>(),
            new List<string>());
    }

    public static IPendingExceptionSlotStore NewCachedPendingExceptionSlotStore(MemoryPendingExceptionSlotStore inner, int ttl, IClock clock)
    {
        return new CachedPendingExceptionSlotStore(inner, PendingExceptionSlotCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PendingExceptionSlot> FindPendingExceptionSlots(IPendingExceptionSlotStore store, PendingExceptionSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
