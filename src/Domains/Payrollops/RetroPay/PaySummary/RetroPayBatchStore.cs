namespace Acme.Workforce.Payrollops.RetroPay.PaySummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay batches by code.</summary>
public interface IRetroPayBatchStore
{
    RetroPayBatch? Find(string code);
    void Save(RetroPayBatch item);
    bool Remove(string code);
    List<RetroPayBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayBatchStore(Dictionary<string, RetroPayBatch> items, List<string> order) : IRetroPayBatchStore
{
    public Dictionary<string, RetroPayBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayBatch item)
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

    public List<RetroPayBatch> List()
    {
        var outValue = new List<RetroPayBatch>();
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
public sealed class CachedRetroPayBatchStore(MemoryRetroPayBatchStore inner, RetroPayBatchCache cache, int hits, int misses) : IRetroPayBatchStore
{
    public MemoryRetroPayBatchStore Inner { get; set; } = inner;
    public RetroPayBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayBatch? Find(string code)
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

    public void Save(RetroPayBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay batches. Empty fields match all.</summary>
public sealed record RetroPayBatchQuery(string Name, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayBatch item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<RetroPayBatch> Apply(List<RetroPayBatch> items)
    {
        var matched = Collections.Filter(items, (RetroPayBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay batch store.</summary>
public static class RetroPayBatchStore
{
    public static IRetroPayBatchStore NewMemoryRetroPayBatchStore()
    {
        return new MemoryRetroPayBatchStore(new Dictionary<string, RetroPayBatch>(), new List<string>());
    }

    public static IRetroPayBatchStore NewCachedRetroPayBatchStore(MemoryRetroPayBatchStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayBatchStore(inner, RetroPayBatchCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RetroPayBatch> FindRetroPayBatches(IRetroPayBatchStore store, RetroPayBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
