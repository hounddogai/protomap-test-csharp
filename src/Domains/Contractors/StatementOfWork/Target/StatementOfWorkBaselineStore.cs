namespace Acme.Workforce.Contractors.StatementOfWork.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work baselines by code.</summary>
public interface IStatementOfWorkBaselineStore
{
    StatementOfWorkBaseline? Find(string code);
    void Save(StatementOfWorkBaseline item);
    bool Remove(string code);
    List<StatementOfWorkBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkBaselineStore(Dictionary<string, StatementOfWorkBaseline> items, List<string> order) : IStatementOfWorkBaselineStore
{
    public Dictionary<string, StatementOfWorkBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkBaseline item)
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

    public List<StatementOfWorkBaseline> List()
    {
        var outValue = new List<StatementOfWorkBaseline>();
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
public sealed class CachedStatementOfWorkBaselineStore(MemoryStatementOfWorkBaselineStore inner, StatementOfWorkBaselineCache cache, int hits, int misses) : IStatementOfWorkBaselineStore
{
    public MemoryStatementOfWorkBaselineStore Inner { get; set; } = inner;
    public StatementOfWorkBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkBaseline? Find(string code)
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

    public void Save(StatementOfWorkBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work baselines. Empty fields match all.</summary>
public sealed record StatementOfWorkBaselineQuery(string Label, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkBaseline item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkBaseline> Apply(List<StatementOfWorkBaseline> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work baseline store.</summary>
public static class StatementOfWorkBaselineStore
{
    public static IStatementOfWorkBaselineStore NewMemoryStatementOfWorkBaselineStore()
    {
        return new MemoryStatementOfWorkBaselineStore(
            new Dictionary<string, StatementOfWorkBaseline>(),
            new List<string>());
    }

    public static IStatementOfWorkBaselineStore NewCachedStatementOfWorkBaselineStore(MemoryStatementOfWorkBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkBaselineStore(
            inner,
            StatementOfWorkBaselineCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<StatementOfWorkBaseline> FindStatementOfWorkBaselines(IStatementOfWorkBaselineStore store, StatementOfWorkBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
