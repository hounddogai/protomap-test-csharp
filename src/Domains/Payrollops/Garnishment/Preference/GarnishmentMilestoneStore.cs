namespace Acme.Workforce.Payrollops.Garnishment.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment milestones by code.</summary>
public interface IGarnishmentMilestoneStore
{
    GarnishmentMilestone? Find(string code);
    void Save(GarnishmentMilestone item);
    bool Remove(string code);
    List<GarnishmentMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentMilestoneStore(Dictionary<string, GarnishmentMilestone> items, List<string> order) : IGarnishmentMilestoneStore
{
    public Dictionary<string, GarnishmentMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentMilestone item)
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

    public List<GarnishmentMilestone> List()
    {
        var outValue = new List<GarnishmentMilestone>();
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
public sealed class CachedGarnishmentMilestoneStore(MemoryGarnishmentMilestoneStore inner, GarnishmentMilestoneCache cache, int hits, int misses) : IGarnishmentMilestoneStore
{
    public MemoryGarnishmentMilestoneStore Inner { get; set; } = inner;
    public GarnishmentMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentMilestone? Find(string code)
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

    public void Save(GarnishmentMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment milestones. Empty fields match all.</summary>
public sealed record GarnishmentMilestoneQuery(string Label, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentMilestone item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GarnishmentMilestone> Apply(List<GarnishmentMilestone> items)
    {
        var matched = Collections.Filter(items, (GarnishmentMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment milestone store.</summary>
public static class GarnishmentMilestoneStore
{
    public static IGarnishmentMilestoneStore NewMemoryGarnishmentMilestoneStore()
    {
        return new MemoryGarnishmentMilestoneStore(
            new Dictionary<string, GarnishmentMilestone>(),
            new List<string>());
    }

    public static IGarnishmentMilestoneStore NewCachedGarnishmentMilestoneStore(MemoryGarnishmentMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentMilestoneStore(inner, GarnishmentMilestoneCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<GarnishmentMilestone> FindGarnishmentMilestones(IGarnishmentMilestoneStore store, GarnishmentMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
