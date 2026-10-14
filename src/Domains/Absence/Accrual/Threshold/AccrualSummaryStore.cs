namespace Acme.Workforce.Absence.Accrual.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual summaries by code.</summary>
public interface IAccrualSummaryStore
{
    AccrualSummary? Find(string code);
    void Save(AccrualSummary item);
    bool Remove(string code);
    List<AccrualSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualSummaryStore(Dictionary<string, AccrualSummary> items, List<string> order) : IAccrualSummaryStore
{
    public Dictionary<string, AccrualSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualSummary item)
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

    public List<AccrualSummary> List()
    {
        var outValue = new List<AccrualSummary>();
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
public sealed class CachedAccrualSummaryStore(MemoryAccrualSummaryStore inner, AccrualSummaryCache cache, int hits, int misses) : IAccrualSummaryStore
{
    public MemoryAccrualSummaryStore Inner { get; set; } = inner;
    public AccrualSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualSummary? Find(string code)
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

    public void Save(AccrualSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual summaries. Empty fields match all.</summary>
public sealed record AccrualSummaryQuery(string Name, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualSummary item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccrualSummary> Apply(List<AccrualSummary> items)
    {
        var matched = Collections.Filter(items, (AccrualSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual summary store.</summary>
public static class AccrualSummaryStore
{
    public static IAccrualSummaryStore NewMemoryAccrualSummaryStore()
    {
        return new MemoryAccrualSummaryStore(new Dictionary<string, AccrualSummary>(), new List<string>());
    }

    public static IAccrualSummaryStore NewCachedAccrualSummaryStore(MemoryAccrualSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualSummaryStore(inner, AccrualSummaryCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AccrualSummary> FindAccrualSummaries(IAccrualSummaryStore store, AccrualSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
