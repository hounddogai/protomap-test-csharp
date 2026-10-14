namespace Acme.Workforce.Compensation.EquityGrant.GrantBatch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant milestones by code.</summary>
public interface IEquityGrantMilestoneStore
{
    EquityGrantMilestone? Find(string code);
    void Save(EquityGrantMilestone item);
    bool Remove(string code);
    List<EquityGrantMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantMilestoneStore(Dictionary<string, EquityGrantMilestone> items, List<string> order) : IEquityGrantMilestoneStore
{
    public Dictionary<string, EquityGrantMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantMilestone item)
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

    public List<EquityGrantMilestone> List()
    {
        var outValue = new List<EquityGrantMilestone>();
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
public sealed class CachedEquityGrantMilestoneStore(MemoryEquityGrantMilestoneStore inner, EquityGrantMilestoneCache cache, int hits, int misses) : IEquityGrantMilestoneStore
{
    public MemoryEquityGrantMilestoneStore Inner { get; set; } = inner;
    public EquityGrantMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantMilestone? Find(string code)
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

    public void Save(EquityGrantMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant milestones. Empty fields match all.</summary>
public sealed record EquityGrantMilestoneQuery(string Title, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantMilestone item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantMilestone> Apply(List<EquityGrantMilestone> items)
    {
        var matched = Collections.Filter(items, (EquityGrantMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant milestone store.</summary>
public static class EquityGrantMilestoneStore
{
    public static IEquityGrantMilestoneStore NewMemoryEquityGrantMilestoneStore()
    {
        return new MemoryEquityGrantMilestoneStore(
            new Dictionary<string, EquityGrantMilestone>(),
            new List<string>());
    }

    public static IEquityGrantMilestoneStore NewCachedEquityGrantMilestoneStore(MemoryEquityGrantMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantMilestoneStore(inner, EquityGrantMilestoneCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<EquityGrantMilestone> FindEquityGrantMilestones(IEquityGrantMilestoneStore store, EquityGrantMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
