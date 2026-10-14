namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work overrides by code.</summary>
public interface IStatementOfWorkOverrideStore
{
    StatementOfWorkOverride? Find(string code);
    void Save(StatementOfWorkOverride item);
    bool Remove(string code);
    List<StatementOfWorkOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkOverrideStore(Dictionary<string, StatementOfWorkOverride> items, List<string> order) : IStatementOfWorkOverrideStore
{
    public Dictionary<string, StatementOfWorkOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkOverride item)
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

    public List<StatementOfWorkOverride> List()
    {
        var outValue = new List<StatementOfWorkOverride>();
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
public sealed class CachedStatementOfWorkOverrideStore(MemoryStatementOfWorkOverrideStore inner, StatementOfWorkOverrideCache cache, int hits, int misses) : IStatementOfWorkOverrideStore
{
    public MemoryStatementOfWorkOverrideStore Inner { get; set; } = inner;
    public StatementOfWorkOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkOverride? Find(string code)
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

    public void Save(StatementOfWorkOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work overrides. Empty fields match all.</summary>
public sealed record StatementOfWorkOverrideQuery(string Country, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkOverride item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkOverride> Apply(List<StatementOfWorkOverride> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work override store.</summary>
public static class StatementOfWorkOverrideStore
{
    public static IStatementOfWorkOverrideStore NewMemoryStatementOfWorkOverrideStore()
    {
        return new MemoryStatementOfWorkOverrideStore(
            new Dictionary<string, StatementOfWorkOverride>(),
            new List<string>());
    }

    public static IStatementOfWorkOverrideStore NewCachedStatementOfWorkOverrideStore(MemoryStatementOfWorkOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkOverrideStore(
            inner,
            StatementOfWorkOverrideCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<StatementOfWorkOverride> FindStatementOfWorkOverrides(IStatementOfWorkOverrideStore store, StatementOfWorkOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
