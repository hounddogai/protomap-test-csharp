namespace Acme.Workforce.Contractors.StatementOfWork.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual statement of work baselines by code.</summary>
public interface IManualStatementOfWorkBaselineStore
{
    ManualStatementOfWorkBaseline? Find(string code);
    void Save(ManualStatementOfWorkBaseline item);
    bool Remove(string code);
    List<ManualStatementOfWorkBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualStatementOfWorkBaselineStore(Dictionary<string, ManualStatementOfWorkBaseline> items, List<string> order) : IManualStatementOfWorkBaselineStore
{
    public Dictionary<string, ManualStatementOfWorkBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualStatementOfWorkBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualStatementOfWorkBaseline item)
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

    public List<ManualStatementOfWorkBaseline> List()
    {
        var outValue = new List<ManualStatementOfWorkBaseline>();
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
public sealed class CachedManualStatementOfWorkBaselineStore(MemoryManualStatementOfWorkBaselineStore inner, ManualStatementOfWorkBaselineCache cache, int hits, int misses) : IManualStatementOfWorkBaselineStore
{
    public MemoryManualStatementOfWorkBaselineStore Inner { get; set; } = inner;
    public ManualStatementOfWorkBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualStatementOfWorkBaseline? Find(string code)
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

    public void Save(ManualStatementOfWorkBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualStatementOfWorkBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual statement of work baselines. Empty fields match all.</summary>
public sealed record ManualStatementOfWorkBaselineQuery(string Unit, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(ManualStatementOfWorkBaseline item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualStatementOfWorkBaseline> Apply(List<ManualStatementOfWorkBaseline> items)
    {
        var matched = Collections.Filter(items, (ManualStatementOfWorkBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual statement of work baseline store.</summary>
public static class ManualStatementOfWorkBaselineStore
{
    public static IManualStatementOfWorkBaselineStore NewMemoryManualStatementOfWorkBaselineStore()
    {
        return new MemoryManualStatementOfWorkBaselineStore(
            new Dictionary<string, ManualStatementOfWorkBaseline>(),
            new List<string>());
    }

    public static IManualStatementOfWorkBaselineStore NewCachedManualStatementOfWorkBaselineStore(MemoryManualStatementOfWorkBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedManualStatementOfWorkBaselineStore(
            inner,
            ManualStatementOfWorkBaselineCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ManualStatementOfWorkBaseline> FindManualStatementOfWorkBaselines(IManualStatementOfWorkBaselineStore store, ManualStatementOfWorkBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
