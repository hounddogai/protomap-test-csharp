namespace Acme.Workforce.Workplace.Desk.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk milestones by code.</summary>
public interface IDeskMilestoneStore
{
    DeskMilestone? Find(string code);
    void Save(DeskMilestone item);
    bool Remove(string code);
    List<DeskMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskMilestoneStore(Dictionary<string, DeskMilestone> items, List<string> order) : IDeskMilestoneStore
{
    public Dictionary<string, DeskMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskMilestone item)
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

    public List<DeskMilestone> List()
    {
        var outValue = new List<DeskMilestone>();
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
public sealed class CachedDeskMilestoneStore(MemoryDeskMilestoneStore inner, DeskMilestoneCache cache, int hits, int misses) : IDeskMilestoneStore
{
    public MemoryDeskMilestoneStore Inner { get; set; } = inner;
    public DeskMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskMilestone? Find(string code)
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

    public void Save(DeskMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk milestones. Empty fields match all.</summary>
public sealed record DeskMilestoneQuery(string Country, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(DeskMilestone item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<DeskMilestone> Apply(List<DeskMilestone> items)
    {
        var matched = Collections.Filter(items, (DeskMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk milestone store.</summary>
public static class DeskMilestoneStore
{
    public static IDeskMilestoneStore NewMemoryDeskMilestoneStore()
    {
        return new MemoryDeskMilestoneStore(new Dictionary<string, DeskMilestone>(), new List<string>());
    }

    public static IDeskMilestoneStore NewCachedDeskMilestoneStore(MemoryDeskMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedDeskMilestoneStore(inner, DeskMilestoneCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<DeskMilestone> FindDeskMilestones(IDeskMilestoneStore store, DeskMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
