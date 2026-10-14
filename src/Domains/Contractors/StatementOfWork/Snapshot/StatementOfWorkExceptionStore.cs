namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work exceptions by code.</summary>
public interface IStatementOfWorkExceptionStore
{
    StatementOfWorkException? Find(string code);
    void Save(StatementOfWorkException item);
    bool Remove(string code);
    List<StatementOfWorkException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkExceptionStore(Dictionary<string, StatementOfWorkException> items, List<string> order) : IStatementOfWorkExceptionStore
{
    public Dictionary<string, StatementOfWorkException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkException item)
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

    public List<StatementOfWorkException> List()
    {
        var outValue = new List<StatementOfWorkException>();
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
public sealed class CachedStatementOfWorkExceptionStore(MemoryStatementOfWorkExceptionStore inner, StatementOfWorkExceptionCache cache, int hits, int misses) : IStatementOfWorkExceptionStore
{
    public MemoryStatementOfWorkExceptionStore Inner { get; set; } = inner;
    public StatementOfWorkExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkException? Find(string code)
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

    public void Save(StatementOfWorkException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work exceptions. Empty fields match all.</summary>
public sealed record StatementOfWorkExceptionQuery(string Category, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkException item)
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

    public List<StatementOfWorkException> Apply(List<StatementOfWorkException> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work exception store.</summary>
public static class StatementOfWorkExceptionStore
{
    public static IStatementOfWorkExceptionStore NewMemoryStatementOfWorkExceptionStore()
    {
        return new MemoryStatementOfWorkExceptionStore(
            new Dictionary<string, StatementOfWorkException>(),
            new List<string>());
    }

    public static IStatementOfWorkExceptionStore NewCachedStatementOfWorkExceptionStore(MemoryStatementOfWorkExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkExceptionStore(
            inner,
            StatementOfWorkExceptionCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<StatementOfWorkException> FindStatementOfWorkExceptions(IStatementOfWorkExceptionStore store, StatementOfWorkExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
