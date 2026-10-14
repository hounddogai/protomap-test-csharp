namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual access badge milestones by code.</summary>
public interface IManualAccessBadgeMilestoneStore
{
    ManualAccessBadgeMilestone? Find(string code);
    void Save(ManualAccessBadgeMilestone item);
    bool Remove(string code);
    List<ManualAccessBadgeMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualAccessBadgeMilestoneStore(Dictionary<string, ManualAccessBadgeMilestone> items, List<string> order) : IManualAccessBadgeMilestoneStore
{
    public Dictionary<string, ManualAccessBadgeMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualAccessBadgeMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualAccessBadgeMilestone item)
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

    public List<ManualAccessBadgeMilestone> List()
    {
        var outValue = new List<ManualAccessBadgeMilestone>();
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
public sealed class CachedManualAccessBadgeMilestoneStore(MemoryManualAccessBadgeMilestoneStore inner, ManualAccessBadgeMilestoneCache cache, int hits, int misses) : IManualAccessBadgeMilestoneStore
{
    public MemoryManualAccessBadgeMilestoneStore Inner { get; set; } = inner;
    public ManualAccessBadgeMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualAccessBadgeMilestone? Find(string code)
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

    public void Save(ManualAccessBadgeMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualAccessBadgeMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual access badge milestones. Empty fields match all.</summary>
public sealed record ManualAccessBadgeMilestoneQuery(string Title, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(ManualAccessBadgeMilestone item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualAccessBadgeMilestone> Apply(List<ManualAccessBadgeMilestone> items)
    {
        var matched = Collections.Filter(items, (ManualAccessBadgeMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual access badge milestone store.</summary>
public static class ManualAccessBadgeMilestoneStore
{
    public static IManualAccessBadgeMilestoneStore NewMemoryManualAccessBadgeMilestoneStore()
    {
        return new MemoryManualAccessBadgeMilestoneStore(
            new Dictionary<string, ManualAccessBadgeMilestone>(),
            new List<string>());
    }

    public static IManualAccessBadgeMilestoneStore NewCachedManualAccessBadgeMilestoneStore(MemoryManualAccessBadgeMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedManualAccessBadgeMilestoneStore(
            inner,
            ManualAccessBadgeMilestoneCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ManualAccessBadgeMilestone> FindManualAccessBadgeMilestones(IManualAccessBadgeMilestoneStore store, ManualAccessBadgeMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
