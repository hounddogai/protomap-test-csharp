namespace Acme.Workforce.Contractors.StatementOfWork.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work records by code.</summary>
public interface IStatementOfWorkRecordStore
{
    StatementOfWorkRecord? Find(string code);
    void Save(StatementOfWorkRecord item);
    bool Remove(string code);
    List<StatementOfWorkRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkRecordStore(Dictionary<string, StatementOfWorkRecord> items, List<string> order) : IStatementOfWorkRecordStore
{
    public Dictionary<string, StatementOfWorkRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkRecord item)
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

    public List<StatementOfWorkRecord> List()
    {
        var outValue = new List<StatementOfWorkRecord>();
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
public sealed class CachedStatementOfWorkRecordStore(MemoryStatementOfWorkRecordStore inner, StatementOfWorkRecordCache cache, int hits, int misses) : IStatementOfWorkRecordStore
{
    public MemoryStatementOfWorkRecordStore Inner { get; set; } = inner;
    public StatementOfWorkRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkRecord? Find(string code)
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

    public void Save(StatementOfWorkRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work records. Empty fields match all.</summary>
public sealed record StatementOfWorkRecordQuery(string Category, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkRecord item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkRecord> Apply(List<StatementOfWorkRecord> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work record store.</summary>
public static class StatementOfWorkRecordStore
{
    public static IStatementOfWorkRecordStore NewMemoryStatementOfWorkRecordStore()
    {
        return new MemoryStatementOfWorkRecordStore(
            new Dictionary<string, StatementOfWorkRecord>(),
            new List<string>());
    }

    public static IStatementOfWorkRecordStore NewCachedStatementOfWorkRecordStore(MemoryStatementOfWorkRecordStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkRecordStore(inner, StatementOfWorkRecordCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<StatementOfWorkRecord> FindStatementOfWorkRecords(IStatementOfWorkRecordStore store, StatementOfWorkRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
