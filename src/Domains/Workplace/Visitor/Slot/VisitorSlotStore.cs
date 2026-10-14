namespace Acme.Workforce.Workplace.Visitor.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor slots by code.</summary>
public interface IVisitorSlotStore
{
    VisitorSlot? Find(string code);
    void Save(VisitorSlot item);
    bool Remove(string code);
    List<VisitorSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorSlotStore(Dictionary<string, VisitorSlot> items, List<string> order) : IVisitorSlotStore
{
    public Dictionary<string, VisitorSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorSlot item)
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

    public List<VisitorSlot> List()
    {
        var outValue = new List<VisitorSlot>();
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
public sealed class CachedVisitorSlotStore(MemoryVisitorSlotStore inner, VisitorSlotCache cache, int hits, int misses) : IVisitorSlotStore
{
    public MemoryVisitorSlotStore Inner { get; set; } = inner;
    public VisitorSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorSlot? Find(string code)
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

    public void Save(VisitorSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor slots. Empty fields match all.</summary>
public sealed record VisitorSlotQuery(string Label, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorSlot item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorSlot> Apply(List<VisitorSlot> items)
    {
        var matched = Collections.Filter(items, (VisitorSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor slot store.</summary>
public static class VisitorSlotStore
{
    public static IVisitorSlotStore NewMemoryVisitorSlotStore()
    {
        return new MemoryVisitorSlotStore(new Dictionary<string, VisitorSlot>(), new List<string>());
    }

    public static IVisitorSlotStore NewCachedVisitorSlotStore(MemoryVisitorSlotStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorSlotStore(inner, VisitorSlotCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<VisitorSlot> FindVisitorSlots(IVisitorSlotStore store, VisitorSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
