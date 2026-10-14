namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work targets by code.</summary>
public interface IStatementOfWorkTargetStore
{
    StatementOfWorkTarget? Find(string code);
    void Save(StatementOfWorkTarget item);
    bool Remove(string code);
    List<StatementOfWorkTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkTargetStore(Dictionary<string, StatementOfWorkTarget> items, List<string> order) : IStatementOfWorkTargetStore
{
    public Dictionary<string, StatementOfWorkTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkTarget item)
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

    public List<StatementOfWorkTarget> List()
    {
        var outValue = new List<StatementOfWorkTarget>();
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
public sealed class CachedStatementOfWorkTargetStore(MemoryStatementOfWorkTargetStore inner, StatementOfWorkTargetCache cache, int hits, int misses) : IStatementOfWorkTargetStore
{
    public MemoryStatementOfWorkTargetStore Inner { get; set; } = inner;
    public StatementOfWorkTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkTarget? Find(string code)
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

    public void Save(StatementOfWorkTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work targets. Empty fields match all.</summary>
public sealed record StatementOfWorkTargetQuery(string Category, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkTarget item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkTarget> Apply(List<StatementOfWorkTarget> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work target store.</summary>
public static class StatementOfWorkTargetStore
{
    public static IStatementOfWorkTargetStore NewMemoryStatementOfWorkTargetStore()
    {
        return new MemoryStatementOfWorkTargetStore(
            new Dictionary<string, StatementOfWorkTarget>(),
            new List<string>());
    }

    public static IStatementOfWorkTargetStore NewCachedStatementOfWorkTargetStore(MemoryStatementOfWorkTargetStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkTargetStore(inner, StatementOfWorkTargetCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<StatementOfWorkTarget> FindStatementOfWorkTargets(IStatementOfWorkTargetStore store, StatementOfWorkTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
