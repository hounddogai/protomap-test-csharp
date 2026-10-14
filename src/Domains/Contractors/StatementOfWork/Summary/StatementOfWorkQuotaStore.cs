namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work quotas by code.</summary>
public interface IStatementOfWorkQuotaStore
{
    StatementOfWorkQuota? Find(string code);
    void Save(StatementOfWorkQuota item);
    bool Remove(string code);
    List<StatementOfWorkQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkQuotaStore(Dictionary<string, StatementOfWorkQuota> items, List<string> order) : IStatementOfWorkQuotaStore
{
    public Dictionary<string, StatementOfWorkQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkQuota item)
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

    public List<StatementOfWorkQuota> List()
    {
        var outValue = new List<StatementOfWorkQuota>();
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
public sealed class CachedStatementOfWorkQuotaStore(MemoryStatementOfWorkQuotaStore inner, StatementOfWorkQuotaCache cache, int hits, int misses) : IStatementOfWorkQuotaStore
{
    public MemoryStatementOfWorkQuotaStore Inner { get; set; } = inner;
    public StatementOfWorkQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkQuota? Find(string code)
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

    public void Save(StatementOfWorkQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work quotas. Empty fields match all.</summary>
public sealed record StatementOfWorkQuotaQuery(string Source, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkQuota item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<StatementOfWorkQuota> Apply(List<StatementOfWorkQuota> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work quota store.</summary>
public static class StatementOfWorkQuotaStore
{
    public static IStatementOfWorkQuotaStore NewMemoryStatementOfWorkQuotaStore()
    {
        return new MemoryStatementOfWorkQuotaStore(
            new Dictionary<string, StatementOfWorkQuota>(),
            new List<string>());
    }

    public static IStatementOfWorkQuotaStore NewCachedStatementOfWorkQuotaStore(MemoryStatementOfWorkQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkQuotaStore(inner, StatementOfWorkQuotaCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<StatementOfWorkQuota> FindStatementOfWorkQuotas(IStatementOfWorkQuotaStore store, StatementOfWorkQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
