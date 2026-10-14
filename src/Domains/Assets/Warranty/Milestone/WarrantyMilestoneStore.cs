namespace Acme.Workforce.Assets.Warranty.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty milestones by code.</summary>
public interface IWarrantyMilestoneStore
{
    WarrantyMilestone? Find(string code);
    void Save(WarrantyMilestone item);
    bool Remove(string code);
    List<WarrantyMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyMilestoneStore(Dictionary<string, WarrantyMilestone> items, List<string> order) : IWarrantyMilestoneStore
{
    public Dictionary<string, WarrantyMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyMilestone item)
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

    public List<WarrantyMilestone> List()
    {
        var outValue = new List<WarrantyMilestone>();
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
public sealed class CachedWarrantyMilestoneStore(MemoryWarrantyMilestoneStore inner, WarrantyMilestoneCache cache, int hits, int misses) : IWarrantyMilestoneStore
{
    public MemoryWarrantyMilestoneStore Inner { get; set; } = inner;
    public WarrantyMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyMilestone? Find(string code)
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

    public void Save(WarrantyMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty milestones. Empty fields match all.</summary>
public sealed record WarrantyMilestoneQuery(string Team, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyMilestone item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyMilestone> Apply(List<WarrantyMilestone> items)
    {
        var matched = Collections.Filter(items, (WarrantyMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty milestone store.</summary>
public static class WarrantyMilestoneStore
{
    public static IWarrantyMilestoneStore NewMemoryWarrantyMilestoneStore()
    {
        return new MemoryWarrantyMilestoneStore(new Dictionary<string, WarrantyMilestone>(), new List<string>());
    }

    public static IWarrantyMilestoneStore NewCachedWarrantyMilestoneStore(MemoryWarrantyMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyMilestoneStore(inner, WarrantyMilestoneCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<WarrantyMilestone> FindWarrantyMilestones(IWarrantyMilestoneStore store, WarrantyMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
