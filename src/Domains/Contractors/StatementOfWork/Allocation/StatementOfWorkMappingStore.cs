namespace Acme.Workforce.Contractors.StatementOfWork.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work mappings by code.</summary>
public interface IStatementOfWorkMappingStore
{
    StatementOfWorkMapping? Find(string code);
    void Save(StatementOfWorkMapping item);
    bool Remove(string code);
    List<StatementOfWorkMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkMappingStore(Dictionary<string, StatementOfWorkMapping> items, List<string> order) : IStatementOfWorkMappingStore
{
    public Dictionary<string, StatementOfWorkMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkMapping item)
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

    public List<StatementOfWorkMapping> List()
    {
        var outValue = new List<StatementOfWorkMapping>();
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
public sealed class CachedStatementOfWorkMappingStore(MemoryStatementOfWorkMappingStore inner, StatementOfWorkMappingCache cache, int hits, int misses) : IStatementOfWorkMappingStore
{
    public MemoryStatementOfWorkMappingStore Inner { get; set; } = inner;
    public StatementOfWorkMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkMapping? Find(string code)
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

    public void Save(StatementOfWorkMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work mappings. Empty fields match all.</summary>
public sealed record StatementOfWorkMappingQuery(string Name, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkMapping item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkMapping> Apply(List<StatementOfWorkMapping> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work mapping store.</summary>
public static class StatementOfWorkMappingStore
{
    public static IStatementOfWorkMappingStore NewMemoryStatementOfWorkMappingStore()
    {
        return new MemoryStatementOfWorkMappingStore(
            new Dictionary<string, StatementOfWorkMapping>(),
            new List<string>());
    }

    public static IStatementOfWorkMappingStore NewCachedStatementOfWorkMappingStore(MemoryStatementOfWorkMappingStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkMappingStore(
            inner,
            StatementOfWorkMappingCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StatementOfWorkMapping> FindStatementOfWorkMappings(IStatementOfWorkMappingStore store, StatementOfWorkMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
