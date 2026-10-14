namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending visitor slots by code.</summary>
public interface IPendingVisitorSlotStore
{
    PendingVisitorSlot? Find(string code);
    void Save(PendingVisitorSlot item);
    bool Remove(string code);
    List<PendingVisitorSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingVisitorSlotStore(Dictionary<string, PendingVisitorSlot> items, List<string> order) : IPendingVisitorSlotStore
{
    public Dictionary<string, PendingVisitorSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingVisitorSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingVisitorSlot item)
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

    public List<PendingVisitorSlot> List()
    {
        var outValue = new List<PendingVisitorSlot>();
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
public sealed class CachedPendingVisitorSlotStore(MemoryPendingVisitorSlotStore inner, PendingVisitorSlotCache cache, int hits, int misses) : IPendingVisitorSlotStore
{
    public MemoryPendingVisitorSlotStore Inner { get; set; } = inner;
    public PendingVisitorSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingVisitorSlot? Find(string code)
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

    public void Save(PendingVisitorSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingVisitorSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending visitor slots. Empty fields match all.</summary>
public sealed record PendingVisitorSlotQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(PendingVisitorSlot item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingVisitorSlot> Apply(List<PendingVisitorSlot> items)
    {
        var matched = Collections.Filter(items, (PendingVisitorSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending visitor slot store.</summary>
public static class PendingVisitorSlotStore
{
    public static IPendingVisitorSlotStore NewMemoryPendingVisitorSlotStore()
    {
        return new MemoryPendingVisitorSlotStore(new Dictionary<string, PendingVisitorSlot>(), new List<string>());
    }

    public static IPendingVisitorSlotStore NewCachedPendingVisitorSlotStore(MemoryPendingVisitorSlotStore inner, int ttl, IClock clock)
    {
        return new CachedPendingVisitorSlotStore(inner, PendingVisitorSlotCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PendingVisitorSlot> FindPendingVisitorSlots(IPendingVisitorSlotStore store, PendingVisitorSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
