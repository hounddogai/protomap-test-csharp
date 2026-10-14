namespace Acme.Workforce.Workplace.Desk.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending desk milestones by code.</summary>
public interface IPendingDeskMilestoneStore
{
    PendingDeskMilestone? Find(string code);
    void Save(PendingDeskMilestone item);
    bool Remove(string code);
    List<PendingDeskMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingDeskMilestoneStore(Dictionary<string, PendingDeskMilestone> items, List<string> order) : IPendingDeskMilestoneStore
{
    public Dictionary<string, PendingDeskMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingDeskMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingDeskMilestone item)
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

    public List<PendingDeskMilestone> List()
    {
        var outValue = new List<PendingDeskMilestone>();
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
public sealed class CachedPendingDeskMilestoneStore(MemoryPendingDeskMilestoneStore inner, PendingDeskMilestoneCache cache, int hits, int misses) : IPendingDeskMilestoneStore
{
    public MemoryPendingDeskMilestoneStore Inner { get; set; } = inner;
    public PendingDeskMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingDeskMilestone? Find(string code)
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

    public void Save(PendingDeskMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingDeskMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending desk milestones. Empty fields match all.</summary>
public sealed record PendingDeskMilestoneQuery(string Segment, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(PendingDeskMilestone item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingDeskMilestone> Apply(List<PendingDeskMilestone> items)
    {
        var matched = Collections.Filter(items, (PendingDeskMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending desk milestone store.</summary>
public static class PendingDeskMilestoneStore
{
    public static IPendingDeskMilestoneStore NewMemoryPendingDeskMilestoneStore()
    {
        return new MemoryPendingDeskMilestoneStore(
            new Dictionary<string, PendingDeskMilestone>(),
            new List<string>());
    }

    public static IPendingDeskMilestoneStore NewCachedPendingDeskMilestoneStore(MemoryPendingDeskMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedPendingDeskMilestoneStore(inner, PendingDeskMilestoneCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PendingDeskMilestone> FindPendingDeskMilestones(IPendingDeskMilestoneStore store, PendingDeskMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
