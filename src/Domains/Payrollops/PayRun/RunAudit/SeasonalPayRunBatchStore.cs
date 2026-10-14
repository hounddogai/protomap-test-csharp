namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal pay run batches by code.</summary>
public interface ISeasonalPayRunBatchStore
{
    SeasonalPayRunBatch? Find(string code);
    void Save(SeasonalPayRunBatch item);
    bool Remove(string code);
    List<SeasonalPayRunBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalPayRunBatchStore(Dictionary<string, SeasonalPayRunBatch> items, List<string> order) : ISeasonalPayRunBatchStore
{
    public Dictionary<string, SeasonalPayRunBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalPayRunBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalPayRunBatch item)
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

    public List<SeasonalPayRunBatch> List()
    {
        var outValue = new List<SeasonalPayRunBatch>();
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
public sealed class CachedSeasonalPayRunBatchStore(MemorySeasonalPayRunBatchStore inner, SeasonalPayRunBatchCache cache, int hits, int misses) : ISeasonalPayRunBatchStore
{
    public MemorySeasonalPayRunBatchStore Inner { get; set; } = inner;
    public SeasonalPayRunBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalPayRunBatch? Find(string code)
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

    public void Save(SeasonalPayRunBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalPayRunBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal pay run batches. Empty fields match all.</summary>
public sealed record SeasonalPayRunBatchQuery(string Region, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalPayRunBatch item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<SeasonalPayRunBatch> Apply(List<SeasonalPayRunBatch> items)
    {
        var matched = Collections.Filter(items, (SeasonalPayRunBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal pay run batch store.</summary>
public static class SeasonalPayRunBatchStore
{
    public static ISeasonalPayRunBatchStore NewMemorySeasonalPayRunBatchStore()
    {
        return new MemorySeasonalPayRunBatchStore(new Dictionary<string, SeasonalPayRunBatch>(), new List<string>());
    }

    public static ISeasonalPayRunBatchStore NewCachedSeasonalPayRunBatchStore(MemorySeasonalPayRunBatchStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalPayRunBatchStore(inner, SeasonalPayRunBatchCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SeasonalPayRunBatch> FindSeasonalPayRunBatches(ISeasonalPayRunBatchStore store, SeasonalPayRunBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
