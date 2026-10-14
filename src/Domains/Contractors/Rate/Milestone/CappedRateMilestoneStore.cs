namespace Acme.Workforce.Contractors.Rate.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped rate milestones by code.</summary>
public interface ICappedRateMilestoneStore
{
    CappedRateMilestone? Find(string code);
    void Save(CappedRateMilestone item);
    bool Remove(string code);
    List<CappedRateMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedRateMilestoneStore(Dictionary<string, CappedRateMilestone> items, List<string> order) : ICappedRateMilestoneStore
{
    public Dictionary<string, CappedRateMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedRateMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedRateMilestone item)
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

    public List<CappedRateMilestone> List()
    {
        var outValue = new List<CappedRateMilestone>();
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
public sealed class CachedCappedRateMilestoneStore(MemoryCappedRateMilestoneStore inner, CappedRateMilestoneCache cache, int hits, int misses) : ICappedRateMilestoneStore
{
    public MemoryCappedRateMilestoneStore Inner { get; set; } = inner;
    public CappedRateMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedRateMilestone? Find(string code)
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

    public void Save(CappedRateMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedRateMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped rate milestones. Empty fields match all.</summary>
public sealed record CappedRateMilestoneQuery(string Category, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(CappedRateMilestone item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<CappedRateMilestone> Apply(List<CappedRateMilestone> items)
    {
        var matched = Collections.Filter(items, (CappedRateMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped rate milestone store.</summary>
public static class CappedRateMilestoneStore
{
    public static ICappedRateMilestoneStore NewMemoryCappedRateMilestoneStore()
    {
        return new MemoryCappedRateMilestoneStore(new Dictionary<string, CappedRateMilestone>(), new List<string>());
    }

    public static ICappedRateMilestoneStore NewCachedCappedRateMilestoneStore(MemoryCappedRateMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedCappedRateMilestoneStore(inner, CappedRateMilestoneCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CappedRateMilestone> FindCappedRateMilestones(ICappedRateMilestoneStore store, CappedRateMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
