namespace Acme.Workforce.Compliance.Exception.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception adjustments by code.</summary>
public interface IExceptionAdjustmentStore
{
    ExceptionAdjustment? Find(string code);
    void Save(ExceptionAdjustment item);
    bool Remove(string code);
    List<ExceptionAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionAdjustmentStore(Dictionary<string, ExceptionAdjustment> items, List<string> order) : IExceptionAdjustmentStore
{
    public Dictionary<string, ExceptionAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionAdjustment item)
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

    public List<ExceptionAdjustment> List()
    {
        var outValue = new List<ExceptionAdjustment>();
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
public sealed class CachedExceptionAdjustmentStore(MemoryExceptionAdjustmentStore inner, ExceptionAdjustmentCache cache, int hits, int misses) : IExceptionAdjustmentStore
{
    public MemoryExceptionAdjustmentStore Inner { get; set; } = inner;
    public ExceptionAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionAdjustment? Find(string code)
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

    public void Save(ExceptionAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception adjustments. Empty fields match all.</summary>
public sealed record ExceptionAdjustmentQuery(string Region, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionAdjustment item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ExceptionAdjustment> Apply(List<ExceptionAdjustment> items)
    {
        var matched = Collections.Filter(items, (ExceptionAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception adjustment store.</summary>
public static class ExceptionAdjustmentStore
{
    public static IExceptionAdjustmentStore NewMemoryExceptionAdjustmentStore()
    {
        return new MemoryExceptionAdjustmentStore(new Dictionary<string, ExceptionAdjustment>(), new List<string>());
    }

    public static IExceptionAdjustmentStore NewCachedExceptionAdjustmentStore(MemoryExceptionAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionAdjustmentStore(inner, ExceptionAdjustmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ExceptionAdjustment> FindExceptionAdjustments(IExceptionAdjustmentStore store, ExceptionAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
