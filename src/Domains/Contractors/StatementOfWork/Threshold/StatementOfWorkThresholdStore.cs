namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work thresholds by code.</summary>
public interface IStatementOfWorkThresholdStore
{
    StatementOfWorkThreshold? Find(string code);
    void Save(StatementOfWorkThreshold item);
    bool Remove(string code);
    List<StatementOfWorkThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkThresholdStore(Dictionary<string, StatementOfWorkThreshold> items, List<string> order) : IStatementOfWorkThresholdStore
{
    public Dictionary<string, StatementOfWorkThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkThreshold item)
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

    public List<StatementOfWorkThreshold> List()
    {
        var outValue = new List<StatementOfWorkThreshold>();
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
public sealed class CachedStatementOfWorkThresholdStore(MemoryStatementOfWorkThresholdStore inner, StatementOfWorkThresholdCache cache, int hits, int misses) : IStatementOfWorkThresholdStore
{
    public MemoryStatementOfWorkThresholdStore Inner { get; set; } = inner;
    public StatementOfWorkThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkThreshold? Find(string code)
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

    public void Save(StatementOfWorkThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work thresholds. Empty fields match all.</summary>
public sealed record StatementOfWorkThresholdQuery(string Label, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkThreshold item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<StatementOfWorkThreshold> Apply(List<StatementOfWorkThreshold> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work threshold store.</summary>
public static class StatementOfWorkThresholdStore
{
    public static IStatementOfWorkThresholdStore NewMemoryStatementOfWorkThresholdStore()
    {
        return new MemoryStatementOfWorkThresholdStore(
            new Dictionary<string, StatementOfWorkThreshold>(),
            new List<string>());
    }

    public static IStatementOfWorkThresholdStore NewCachedStatementOfWorkThresholdStore(MemoryStatementOfWorkThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkThresholdStore(
            inner,
            StatementOfWorkThresholdCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<StatementOfWorkThreshold> FindStatementOfWorkThresholds(IStatementOfWorkThresholdStore store, StatementOfWorkThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
