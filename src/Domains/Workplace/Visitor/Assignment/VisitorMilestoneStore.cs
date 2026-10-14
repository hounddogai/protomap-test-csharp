namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor milestones by code.</summary>
public interface IVisitorMilestoneStore
{
    VisitorMilestone? Find(string code);
    void Save(VisitorMilestone item);
    bool Remove(string code);
    List<VisitorMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorMilestoneStore(Dictionary<string, VisitorMilestone> items, List<string> order) : IVisitorMilestoneStore
{
    public Dictionary<string, VisitorMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorMilestone item)
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

    public List<VisitorMilestone> List()
    {
        var outValue = new List<VisitorMilestone>();
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
public sealed class CachedVisitorMilestoneStore(MemoryVisitorMilestoneStore inner, VisitorMilestoneCache cache, int hits, int misses) : IVisitorMilestoneStore
{
    public MemoryVisitorMilestoneStore Inner { get; set; } = inner;
    public VisitorMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorMilestone? Find(string code)
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

    public void Save(VisitorMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor milestones. Empty fields match all.</summary>
public sealed record VisitorMilestoneQuery(string Team, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorMilestone item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorMilestone> Apply(List<VisitorMilestone> items)
    {
        var matched = Collections.Filter(items, (VisitorMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor milestone store.</summary>
public static class VisitorMilestoneStore
{
    public static IVisitorMilestoneStore NewMemoryVisitorMilestoneStore()
    {
        return new MemoryVisitorMilestoneStore(new Dictionary<string, VisitorMilestone>(), new List<string>());
    }

    public static IVisitorMilestoneStore NewCachedVisitorMilestoneStore(MemoryVisitorMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorMilestoneStore(inner, VisitorMilestoneCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<VisitorMilestone> FindVisitorMilestones(IVisitorMilestoneStore store, VisitorMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
