namespace Acme.Workforce.Contractors.StatementOfWork.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work rules by code.</summary>
public interface IStatementOfWorkRuleStore
{
    StatementOfWorkRule? Find(string code);
    void Save(StatementOfWorkRule item);
    bool Remove(string code);
    List<StatementOfWorkRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkRuleStore(Dictionary<string, StatementOfWorkRule> items, List<string> order) : IStatementOfWorkRuleStore
{
    public Dictionary<string, StatementOfWorkRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkRule item)
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

    public List<StatementOfWorkRule> List()
    {
        var outValue = new List<StatementOfWorkRule>();
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
public sealed class CachedStatementOfWorkRuleStore(MemoryStatementOfWorkRuleStore inner, StatementOfWorkRuleCache cache, int hits, int misses) : IStatementOfWorkRuleStore
{
    public MemoryStatementOfWorkRuleStore Inner { get; set; } = inner;
    public StatementOfWorkRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkRule? Find(string code)
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

    public void Save(StatementOfWorkRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work rules. Empty fields match all.</summary>
public sealed record StatementOfWorkRuleQuery(string Region, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkRule item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkRule> Apply(List<StatementOfWorkRule> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work rule store.</summary>
public static class StatementOfWorkRuleStore
{
    public static IStatementOfWorkRuleStore NewMemoryStatementOfWorkRuleStore()
    {
        return new MemoryStatementOfWorkRuleStore(new Dictionary<string, StatementOfWorkRule>(), new List<string>());
    }

    public static IStatementOfWorkRuleStore NewCachedStatementOfWorkRuleStore(MemoryStatementOfWorkRuleStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkRuleStore(inner, StatementOfWorkRuleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<StatementOfWorkRule> FindStatementOfWorkRules(IStatementOfWorkRuleStore store, StatementOfWorkRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
