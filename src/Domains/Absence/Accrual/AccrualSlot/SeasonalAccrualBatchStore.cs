namespace Acme.Workforce.Absence.Accrual.AccrualSlot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal accrual batches by code.</summary>
public interface ISeasonalAccrualBatchStore
{
    SeasonalAccrualBatch? Find(string code);
    void Save(SeasonalAccrualBatch item);
    bool Remove(string code);
    List<SeasonalAccrualBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalAccrualBatchStore(Dictionary<string, SeasonalAccrualBatch> items, List<string> order) : ISeasonalAccrualBatchStore
{
    public Dictionary<string, SeasonalAccrualBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalAccrualBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalAccrualBatch item)
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

    public List<SeasonalAccrualBatch> List()
    {
        var outValue = new List<SeasonalAccrualBatch>();
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
public sealed class CachedSeasonalAccrualBatchStore(MemorySeasonalAccrualBatchStore inner, SeasonalAccrualBatchCache cache, int hits, int misses) : ISeasonalAccrualBatchStore
{
    public MemorySeasonalAccrualBatchStore Inner { get; set; } = inner;
    public SeasonalAccrualBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalAccrualBatch? Find(string code)
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

    public void Save(SeasonalAccrualBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalAccrualBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal accrual batches. Empty fields match all.</summary>
public sealed record SeasonalAccrualBatchQuery(string Region, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalAccrualBatch item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalAccrualBatch> Apply(List<SeasonalAccrualBatch> items)
    {
        var matched = Collections.Filter(items, (SeasonalAccrualBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal accrual batch store.</summary>
public static class SeasonalAccrualBatchStore
{
    public static ISeasonalAccrualBatchStore NewMemorySeasonalAccrualBatchStore()
    {
        return new MemorySeasonalAccrualBatchStore(
            new Dictionary<string, SeasonalAccrualBatch>(),
            new List<string>());
    }

    public static ISeasonalAccrualBatchStore NewCachedSeasonalAccrualBatchStore(MemorySeasonalAccrualBatchStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalAccrualBatchStore(inner, SeasonalAccrualBatchCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SeasonalAccrualBatch> FindSeasonalAccrualBatches(ISeasonalAccrualBatchStore store, SeasonalAccrualBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
