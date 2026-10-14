namespace Acme.Workforce.Assets.Refresh.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh milestones by code.</summary>
public interface IRefreshMilestoneStore
{
    RefreshMilestone? Find(string code);
    void Save(RefreshMilestone item);
    bool Remove(string code);
    List<RefreshMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshMilestoneStore(Dictionary<string, RefreshMilestone> items, List<string> order) : IRefreshMilestoneStore
{
    public Dictionary<string, RefreshMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshMilestone item)
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

    public List<RefreshMilestone> List()
    {
        var outValue = new List<RefreshMilestone>();
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
public sealed class CachedRefreshMilestoneStore(MemoryRefreshMilestoneStore inner, RefreshMilestoneCache cache, int hits, int misses) : IRefreshMilestoneStore
{
    public MemoryRefreshMilestoneStore Inner { get; set; } = inner;
    public RefreshMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshMilestone? Find(string code)
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

    public void Save(RefreshMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh milestones. Empty fields match all.</summary>
public sealed record RefreshMilestoneQuery(string Category, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshMilestone item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RefreshMilestone> Apply(List<RefreshMilestone> items)
    {
        var matched = Collections.Filter(items, (RefreshMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh milestone store.</summary>
public static class RefreshMilestoneStore
{
    public static IRefreshMilestoneStore NewMemoryRefreshMilestoneStore()
    {
        return new MemoryRefreshMilestoneStore(new Dictionary<string, RefreshMilestone>(), new List<string>());
    }

    public static IRefreshMilestoneStore NewCachedRefreshMilestoneStore(MemoryRefreshMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshMilestoneStore(inner, RefreshMilestoneCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RefreshMilestone> FindRefreshMilestones(IRefreshMilestoneStore store, RefreshMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
