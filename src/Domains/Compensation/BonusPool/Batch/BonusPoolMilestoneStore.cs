namespace Acme.Workforce.Compensation.BonusPool.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool milestones by code.</summary>
public interface IBonusPoolMilestoneStore
{
    BonusPoolMilestone? Find(string code);
    void Save(BonusPoolMilestone item);
    bool Remove(string code);
    List<BonusPoolMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolMilestoneStore(Dictionary<string, BonusPoolMilestone> items, List<string> order) : IBonusPoolMilestoneStore
{
    public Dictionary<string, BonusPoolMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolMilestone item)
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

    public List<BonusPoolMilestone> List()
    {
        var outValue = new List<BonusPoolMilestone>();
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
public sealed class CachedBonusPoolMilestoneStore(MemoryBonusPoolMilestoneStore inner, BonusPoolMilestoneCache cache, int hits, int misses) : IBonusPoolMilestoneStore
{
    public MemoryBonusPoolMilestoneStore Inner { get; set; } = inner;
    public BonusPoolMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolMilestone? Find(string code)
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

    public void Save(BonusPoolMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool milestones. Empty fields match all.</summary>
public sealed record BonusPoolMilestoneQuery(string Title, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolMilestone item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolMilestone> Apply(List<BonusPoolMilestone> items)
    {
        var matched = Collections.Filter(items, (BonusPoolMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool milestone store.</summary>
public static class BonusPoolMilestoneStore
{
    public static IBonusPoolMilestoneStore NewMemoryBonusPoolMilestoneStore()
    {
        return new MemoryBonusPoolMilestoneStore(new Dictionary<string, BonusPoolMilestone>(), new List<string>());
    }

    public static IBonusPoolMilestoneStore NewCachedBonusPoolMilestoneStore(MemoryBonusPoolMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolMilestoneStore(inner, BonusPoolMilestoneCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BonusPoolMilestone> FindBonusPoolMilestones(IBonusPoolMilestoneStore store, BonusPoolMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
