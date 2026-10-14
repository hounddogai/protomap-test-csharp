namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient balance batches by code.</summary>
public interface ILenientBalanceBatchStore
{
    LenientBalanceBatch? Find(string code);
    void Save(LenientBalanceBatch item);
    bool Remove(string code);
    List<LenientBalanceBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientBalanceBatchStore(Dictionary<string, LenientBalanceBatch> items, List<string> order) : ILenientBalanceBatchStore
{
    public Dictionary<string, LenientBalanceBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientBalanceBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientBalanceBatch item)
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

    public List<LenientBalanceBatch> List()
    {
        var outValue = new List<LenientBalanceBatch>();
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
public sealed class CachedLenientBalanceBatchStore(MemoryLenientBalanceBatchStore inner, LenientBalanceBatchCache cache, int hits, int misses) : ILenientBalanceBatchStore
{
    public MemoryLenientBalanceBatchStore Inner { get; set; } = inner;
    public LenientBalanceBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientBalanceBatch? Find(string code)
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

    public void Save(LenientBalanceBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientBalanceBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient balance batches. Empty fields match all.</summary>
public sealed record LenientBalanceBatchQuery(string Title, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(LenientBalanceBatch item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientBalanceBatch> Apply(List<LenientBalanceBatch> items)
    {
        var matched = Collections.Filter(items, (LenientBalanceBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient balance batch store.</summary>
public static class LenientBalanceBatchStore
{
    public static ILenientBalanceBatchStore NewMemoryLenientBalanceBatchStore()
    {
        return new MemoryLenientBalanceBatchStore(new Dictionary<string, LenientBalanceBatch>(), new List<string>());
    }

    public static ILenientBalanceBatchStore NewCachedLenientBalanceBatchStore(MemoryLenientBalanceBatchStore inner, int ttl, IClock clock)
    {
        return new CachedLenientBalanceBatchStore(inner, LenientBalanceBatchCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LenientBalanceBatch> FindLenientBalanceBatches(ILenientBalanceBatchStore store, LenientBalanceBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
