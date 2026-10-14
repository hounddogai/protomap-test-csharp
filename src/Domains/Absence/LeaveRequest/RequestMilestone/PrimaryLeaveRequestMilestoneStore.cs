namespace Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary leave request milestones by code.</summary>
public interface IPrimaryLeaveRequestMilestoneStore
{
    PrimaryLeaveRequestMilestone? Find(string code);
    void Save(PrimaryLeaveRequestMilestone item);
    bool Remove(string code);
    List<PrimaryLeaveRequestMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryLeaveRequestMilestoneStore(Dictionary<string, PrimaryLeaveRequestMilestone> items, List<string> order) : IPrimaryLeaveRequestMilestoneStore
{
    public Dictionary<string, PrimaryLeaveRequestMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryLeaveRequestMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryLeaveRequestMilestone item)
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

    public List<PrimaryLeaveRequestMilestone> List()
    {
        var outValue = new List<PrimaryLeaveRequestMilestone>();
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
public sealed class CachedPrimaryLeaveRequestMilestoneStore(MemoryPrimaryLeaveRequestMilestoneStore inner, PrimaryLeaveRequestMilestoneCache cache, int hits, int misses) : IPrimaryLeaveRequestMilestoneStore
{
    public MemoryPrimaryLeaveRequestMilestoneStore Inner { get; set; } = inner;
    public PrimaryLeaveRequestMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryLeaveRequestMilestone? Find(string code)
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

    public void Save(PrimaryLeaveRequestMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryLeaveRequestMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary leave request milestones. Empty fields match all.</summary>
public sealed record PrimaryLeaveRequestMilestoneQuery(string Title, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryLeaveRequestMilestone item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryLeaveRequestMilestone> Apply(List<PrimaryLeaveRequestMilestone> items)
    {
        var matched = Collections.Filter(items, (PrimaryLeaveRequestMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary leave request milestone store.</summary>
public static class PrimaryLeaveRequestMilestoneStore
{
    public static IPrimaryLeaveRequestMilestoneStore NewMemoryPrimaryLeaveRequestMilestoneStore()
    {
        return new MemoryPrimaryLeaveRequestMilestoneStore(
            new Dictionary<string, PrimaryLeaveRequestMilestone>(),
            new List<string>());
    }

    public static IPrimaryLeaveRequestMilestoneStore NewCachedPrimaryLeaveRequestMilestoneStore(MemoryPrimaryLeaveRequestMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryLeaveRequestMilestoneStore(
            inner,
            PrimaryLeaveRequestMilestoneCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryLeaveRequestMilestone> FindPrimaryLeaveRequestMilestones(IPrimaryLeaveRequestMilestoneStore store, PrimaryLeaveRequestMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
