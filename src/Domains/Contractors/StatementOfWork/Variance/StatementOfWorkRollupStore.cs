namespace Acme.Workforce.Contractors.StatementOfWork.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work rollups by code.</summary>
public interface IStatementOfWorkRollupStore
{
    StatementOfWorkRollup? Find(string code);
    void Save(StatementOfWorkRollup item);
    bool Remove(string code);
    List<StatementOfWorkRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkRollupStore(Dictionary<string, StatementOfWorkRollup> items, List<string> order) : IStatementOfWorkRollupStore
{
    public Dictionary<string, StatementOfWorkRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkRollup item)
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

    public List<StatementOfWorkRollup> List()
    {
        var outValue = new List<StatementOfWorkRollup>();
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
public sealed class CachedStatementOfWorkRollupStore(MemoryStatementOfWorkRollupStore inner, StatementOfWorkRollupCache cache, int hits, int misses) : IStatementOfWorkRollupStore
{
    public MemoryStatementOfWorkRollupStore Inner { get; set; } = inner;
    public StatementOfWorkRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkRollup? Find(string code)
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

    public void Save(StatementOfWorkRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work rollups. Empty fields match all.</summary>
public sealed record StatementOfWorkRollupQuery(string Region, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkRollup item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkRollup> Apply(List<StatementOfWorkRollup> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work rollup store.</summary>
public static class StatementOfWorkRollupStore
{
    public static IStatementOfWorkRollupStore NewMemoryStatementOfWorkRollupStore()
    {
        return new MemoryStatementOfWorkRollupStore(
            new Dictionary<string, StatementOfWorkRollup>(),
            new List<string>());
    }

    public static IStatementOfWorkRollupStore NewCachedStatementOfWorkRollupStore(MemoryStatementOfWorkRollupStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkRollupStore(
            inner,
            StatementOfWorkRollupCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StatementOfWorkRollup> FindStatementOfWorkRollups(IStatementOfWorkRollupStore store, StatementOfWorkRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
