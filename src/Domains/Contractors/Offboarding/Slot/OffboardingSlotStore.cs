namespace Acme.Workforce.Contractors.Offboarding.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding slots by code.</summary>
public interface IOffboardingSlotStore
{
    OffboardingSlot? Find(string code);
    void Save(OffboardingSlot item);
    bool Remove(string code);
    List<OffboardingSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingSlotStore(Dictionary<string, OffboardingSlot> items, List<string> order) : IOffboardingSlotStore
{
    public Dictionary<string, OffboardingSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingSlot item)
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

    public List<OffboardingSlot> List()
    {
        var outValue = new List<OffboardingSlot>();
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
public sealed class CachedOffboardingSlotStore(MemoryOffboardingSlotStore inner, OffboardingSlotCache cache, int hits, int misses) : IOffboardingSlotStore
{
    public MemoryOffboardingSlotStore Inner { get; set; } = inner;
    public OffboardingSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingSlot? Find(string code)
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

    public void Save(OffboardingSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding slots. Empty fields match all.</summary>
public sealed record OffboardingSlotQuery(string Name, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingSlot item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OffboardingSlot> Apply(List<OffboardingSlot> items)
    {
        var matched = Collections.Filter(items, (OffboardingSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding slot store.</summary>
public static class OffboardingSlotStore
{
    public static IOffboardingSlotStore NewMemoryOffboardingSlotStore()
    {
        return new MemoryOffboardingSlotStore(new Dictionary<string, OffboardingSlot>(), new List<string>());
    }

    public static IOffboardingSlotStore NewCachedOffboardingSlotStore(MemoryOffboardingSlotStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingSlotStore(inner, OffboardingSlotCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<OffboardingSlot> FindOffboardingSlots(IOffboardingSlotStore store, OffboardingSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
