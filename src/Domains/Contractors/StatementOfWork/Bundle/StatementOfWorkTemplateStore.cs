namespace Acme.Workforce.Contractors.StatementOfWork.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work templates by code.</summary>
public interface IStatementOfWorkTemplateStore
{
    StatementOfWorkTemplate? Find(string code);
    void Save(StatementOfWorkTemplate item);
    bool Remove(string code);
    List<StatementOfWorkTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkTemplateStore(Dictionary<string, StatementOfWorkTemplate> items, List<string> order) : IStatementOfWorkTemplateStore
{
    public Dictionary<string, StatementOfWorkTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkTemplate item)
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

    public List<StatementOfWorkTemplate> List()
    {
        var outValue = new List<StatementOfWorkTemplate>();
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
public sealed class CachedStatementOfWorkTemplateStore(MemoryStatementOfWorkTemplateStore inner, StatementOfWorkTemplateCache cache, int hits, int misses) : IStatementOfWorkTemplateStore
{
    public MemoryStatementOfWorkTemplateStore Inner { get; set; } = inner;
    public StatementOfWorkTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkTemplate? Find(string code)
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

    public void Save(StatementOfWorkTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work templates. Empty fields match all.</summary>
public sealed record StatementOfWorkTemplateQuery(string Department, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkTemplate item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkTemplate> Apply(List<StatementOfWorkTemplate> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work template store.</summary>
public static class StatementOfWorkTemplateStore
{
    public static IStatementOfWorkTemplateStore NewMemoryStatementOfWorkTemplateStore()
    {
        return new MemoryStatementOfWorkTemplateStore(
            new Dictionary<string, StatementOfWorkTemplate>(),
            new List<string>());
    }

    public static IStatementOfWorkTemplateStore NewCachedStatementOfWorkTemplateStore(MemoryStatementOfWorkTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkTemplateStore(
            inner,
            StatementOfWorkTemplateCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StatementOfWorkTemplate> FindStatementOfWorkTemplates(IStatementOfWorkTemplateStore store, StatementOfWorkTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
