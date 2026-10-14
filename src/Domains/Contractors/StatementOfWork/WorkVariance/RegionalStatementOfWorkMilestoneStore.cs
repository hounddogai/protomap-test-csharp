namespace Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional statement of work milestones by code.</summary>
public interface IRegionalStatementOfWorkMilestoneStore
{
    RegionalStatementOfWorkMilestone? Find(string code);
    void Save(RegionalStatementOfWorkMilestone item);
    bool Remove(string code);
    List<RegionalStatementOfWorkMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalStatementOfWorkMilestoneStore(Dictionary<string, RegionalStatementOfWorkMilestone> items, List<string> order) : IRegionalStatementOfWorkMilestoneStore
{
    public Dictionary<string, RegionalStatementOfWorkMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalStatementOfWorkMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalStatementOfWorkMilestone item)
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

    public List<RegionalStatementOfWorkMilestone> List()
    {
        var outValue = new List<RegionalStatementOfWorkMilestone>();
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
public sealed class CachedRegionalStatementOfWorkMilestoneStore(MemoryRegionalStatementOfWorkMilestoneStore inner, RegionalStatementOfWorkMilestoneCache cache, int hits, int misses) : IRegionalStatementOfWorkMilestoneStore
{
    public MemoryRegionalStatementOfWorkMilestoneStore Inner { get; set; } = inner;
    public RegionalStatementOfWorkMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalStatementOfWorkMilestone? Find(string code)
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

    public void Save(RegionalStatementOfWorkMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalStatementOfWorkMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional statement of work milestones. Empty fields match all.</summary>
public sealed record RegionalStatementOfWorkMilestoneQuery(string Channel, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalStatementOfWorkMilestone item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalStatementOfWorkMilestone> Apply(List<RegionalStatementOfWorkMilestone> items)
    {
        var matched = Collections.Filter(items, (RegionalStatementOfWorkMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional statement of work milestone store.</summary>
public static class RegionalStatementOfWorkMilestoneStore
{
    public static IRegionalStatementOfWorkMilestoneStore NewMemoryRegionalStatementOfWorkMilestoneStore()
    {
        return new MemoryRegionalStatementOfWorkMilestoneStore(
            new Dictionary<string, RegionalStatementOfWorkMilestone>(),
            new List<string>());
    }

    public static IRegionalStatementOfWorkMilestoneStore NewCachedRegionalStatementOfWorkMilestoneStore(MemoryRegionalStatementOfWorkMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalStatementOfWorkMilestoneStore(
            inner,
            RegionalStatementOfWorkMilestoneCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RegionalStatementOfWorkMilestone> FindRegionalStatementOfWorkMilestones(IRegionalStatementOfWorkMilestoneStore store, RegionalStatementOfWorkMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
