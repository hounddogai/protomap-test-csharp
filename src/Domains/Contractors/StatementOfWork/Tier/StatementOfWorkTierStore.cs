namespace Acme.Workforce.Contractors.StatementOfWork.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work tiers by code.</summary>
public interface IStatementOfWorkTierStore
{
    StatementOfWorkTier? Find(string code);
    void Save(StatementOfWorkTier item);
    bool Remove(string code);
    List<StatementOfWorkTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkTierStore(Dictionary<string, StatementOfWorkTier> items, List<string> order) : IStatementOfWorkTierStore
{
    public Dictionary<string, StatementOfWorkTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkTier item)
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

    public List<StatementOfWorkTier> List()
    {
        var outValue = new List<StatementOfWorkTier>();
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
public sealed class CachedStatementOfWorkTierStore(MemoryStatementOfWorkTierStore inner, StatementOfWorkTierCache cache, int hits, int misses) : IStatementOfWorkTierStore
{
    public MemoryStatementOfWorkTierStore Inner { get; set; } = inner;
    public StatementOfWorkTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkTier? Find(string code)
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

    public void Save(StatementOfWorkTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work tiers. Empty fields match all.</summary>
public sealed record StatementOfWorkTierQuery(string Label, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkTier item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkTier> Apply(List<StatementOfWorkTier> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work tier store.</summary>
public static class StatementOfWorkTierStore
{
    public static IStatementOfWorkTierStore NewMemoryStatementOfWorkTierStore()
    {
        return new MemoryStatementOfWorkTierStore(new Dictionary<string, StatementOfWorkTier>(), new List<string>());
    }

    public static IStatementOfWorkTierStore NewCachedStatementOfWorkTierStore(MemoryStatementOfWorkTierStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkTierStore(inner, StatementOfWorkTierCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<StatementOfWorkTier> FindStatementOfWorkTiers(IStatementOfWorkTierStore store, StatementOfWorkTierQuery query)
    {
        return query.Apply(store.List());
    }
}
