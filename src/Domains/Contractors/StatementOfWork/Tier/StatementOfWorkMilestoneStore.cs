namespace Acme.Workforce.Contractors.StatementOfWork.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work milestones by code.</summary>
public interface IStatementOfWorkMilestoneStore
{
    StatementOfWorkMilestone? Find(string code);
    void Save(StatementOfWorkMilestone item);
    bool Remove(string code);
    List<StatementOfWorkMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkMilestoneStore(Dictionary<string, StatementOfWorkMilestone> items, List<string> order) : IStatementOfWorkMilestoneStore
{
    public Dictionary<string, StatementOfWorkMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkMilestone item)
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

    public List<StatementOfWorkMilestone> List()
    {
        var outValue = new List<StatementOfWorkMilestone>();
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
public sealed class CachedStatementOfWorkMilestoneStore(MemoryStatementOfWorkMilestoneStore inner, StatementOfWorkMilestoneCache cache, int hits, int misses) : IStatementOfWorkMilestoneStore
{
    public MemoryStatementOfWorkMilestoneStore Inner { get; set; } = inner;
    public StatementOfWorkMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkMilestone? Find(string code)
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

    public void Save(StatementOfWorkMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work milestones. Empty fields match all.</summary>
public sealed record StatementOfWorkMilestoneQuery(string Name, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkMilestone item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<StatementOfWorkMilestone> Apply(List<StatementOfWorkMilestone> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work milestone store.</summary>
public static class StatementOfWorkMilestoneStore
{
    public static IStatementOfWorkMilestoneStore NewMemoryStatementOfWorkMilestoneStore()
    {
        return new MemoryStatementOfWorkMilestoneStore(
            new Dictionary<string, StatementOfWorkMilestone>(),
            new List<string>());
    }

    public static IStatementOfWorkMilestoneStore NewCachedStatementOfWorkMilestoneStore(MemoryStatementOfWorkMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkMilestoneStore(
            inner,
            StatementOfWorkMilestoneCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<StatementOfWorkMilestone> FindStatementOfWorkMilestones(IStatementOfWorkMilestoneStore store, StatementOfWorkMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
