namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work policies by code.</summary>
public interface IStatementOfWorkPolicyStore
{
    StatementOfWorkPolicy? Find(string code);
    void Save(StatementOfWorkPolicy item);
    bool Remove(string code);
    List<StatementOfWorkPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkPolicyStore(Dictionary<string, StatementOfWorkPolicy> items, List<string> order) : IStatementOfWorkPolicyStore
{
    public Dictionary<string, StatementOfWorkPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkPolicy item)
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

    public List<StatementOfWorkPolicy> List()
    {
        var outValue = new List<StatementOfWorkPolicy>();
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
public sealed class CachedStatementOfWorkPolicyStore(MemoryStatementOfWorkPolicyStore inner, StatementOfWorkPolicyCache cache, int hits, int misses) : IStatementOfWorkPolicyStore
{
    public MemoryStatementOfWorkPolicyStore Inner { get; set; } = inner;
    public StatementOfWorkPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkPolicy? Find(string code)
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

    public void Save(StatementOfWorkPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work policies. Empty fields match all.</summary>
public sealed record StatementOfWorkPolicyQuery(string Locale, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkPolicy item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkPolicy> Apply(List<StatementOfWorkPolicy> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work policy store.</summary>
public static class StatementOfWorkPolicyStore
{
    public static IStatementOfWorkPolicyStore NewMemoryStatementOfWorkPolicyStore()
    {
        return new MemoryStatementOfWorkPolicyStore(
            new Dictionary<string, StatementOfWorkPolicy>(),
            new List<string>());
    }

    public static IStatementOfWorkPolicyStore NewCachedStatementOfWorkPolicyStore(MemoryStatementOfWorkPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkPolicyStore(inner, StatementOfWorkPolicyCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<StatementOfWorkPolicy> FindStatementOfWorkPolicies(IStatementOfWorkPolicyStore store, StatementOfWorkPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
