namespace Acme.Workforce.Compliance.Exception.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception targets by code.</summary>
public interface IExceptionTargetStore
{
    ExceptionTarget? Find(string code);
    void Save(ExceptionTarget item);
    bool Remove(string code);
    List<ExceptionTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionTargetStore(Dictionary<string, ExceptionTarget> items, List<string> order) : IExceptionTargetStore
{
    public Dictionary<string, ExceptionTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionTarget item)
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

    public List<ExceptionTarget> List()
    {
        var outValue = new List<ExceptionTarget>();
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
public sealed class CachedExceptionTargetStore(MemoryExceptionTargetStore inner, ExceptionTargetCache cache, int hits, int misses) : IExceptionTargetStore
{
    public MemoryExceptionTargetStore Inner { get; set; } = inner;
    public ExceptionTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionTarget? Find(string code)
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

    public void Save(ExceptionTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception targets. Empty fields match all.</summary>
public sealed record ExceptionTargetQuery(string Channel, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionTarget item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ExceptionTarget> Apply(List<ExceptionTarget> items)
    {
        var matched = Collections.Filter(items, (ExceptionTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception target store.</summary>
public static class ExceptionTargetStore
{
    public static IExceptionTargetStore NewMemoryExceptionTargetStore()
    {
        return new MemoryExceptionTargetStore(new Dictionary<string, ExceptionTarget>(), new List<string>());
    }

    public static IExceptionTargetStore NewCachedExceptionTargetStore(MemoryExceptionTargetStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionTargetStore(inner, ExceptionTargetCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ExceptionTarget> FindExceptionTargets(IExceptionTargetStore store, ExceptionTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
