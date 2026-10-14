namespace Acme.Workforce.Assets.Assignment.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled assignment slots by code.</summary>
public interface IScheduledAssignmentSlotStore
{
    ScheduledAssignmentSlot? Find(string code);
    void Save(ScheduledAssignmentSlot item);
    bool Remove(string code);
    List<ScheduledAssignmentSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledAssignmentSlotStore(Dictionary<string, ScheduledAssignmentSlot> items, List<string> order) : IScheduledAssignmentSlotStore
{
    public Dictionary<string, ScheduledAssignmentSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledAssignmentSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledAssignmentSlot item)
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

    public List<ScheduledAssignmentSlot> List()
    {
        var outValue = new List<ScheduledAssignmentSlot>();
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
public sealed class CachedScheduledAssignmentSlotStore(MemoryScheduledAssignmentSlotStore inner, ScheduledAssignmentSlotCache cache, int hits, int misses) : IScheduledAssignmentSlotStore
{
    public MemoryScheduledAssignmentSlotStore Inner { get; set; } = inner;
    public ScheduledAssignmentSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledAssignmentSlot? Find(string code)
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

    public void Save(ScheduledAssignmentSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledAssignmentSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled assignment slots. Empty fields match all.</summary>
public sealed record ScheduledAssignmentSlotQuery(string Title, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledAssignmentSlot item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledAssignmentSlot> Apply(List<ScheduledAssignmentSlot> items)
    {
        var matched = Collections.Filter(items, (ScheduledAssignmentSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled assignment slot store.</summary>
public static class ScheduledAssignmentSlotStore
{
    public static IScheduledAssignmentSlotStore NewMemoryScheduledAssignmentSlotStore()
    {
        return new MemoryScheduledAssignmentSlotStore(
            new Dictionary<string, ScheduledAssignmentSlot>(),
            new List<string>());
    }

    public static IScheduledAssignmentSlotStore NewCachedScheduledAssignmentSlotStore(MemoryScheduledAssignmentSlotStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledAssignmentSlotStore(
            inner,
            ScheduledAssignmentSlotCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledAssignmentSlot> FindScheduledAssignmentSlots(IScheduledAssignmentSlotStore store, ScheduledAssignmentSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
