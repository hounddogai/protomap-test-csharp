namespace Acme.Workforce.Compliance.Exception.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception overrides by code.</summary>
public interface IExceptionOverrideStore
{
    ExceptionOverride? Find(string code);
    void Save(ExceptionOverride item);
    bool Remove(string code);
    List<ExceptionOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionOverrideStore(Dictionary<string, ExceptionOverride> items, List<string> order) : IExceptionOverrideStore
{
    public Dictionary<string, ExceptionOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionOverride item)
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

    public List<ExceptionOverride> List()
    {
        var outValue = new List<ExceptionOverride>();
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
public sealed class CachedExceptionOverrideStore(MemoryExceptionOverrideStore inner, ExceptionOverrideCache cache, int hits, int misses) : IExceptionOverrideStore
{
    public MemoryExceptionOverrideStore Inner { get; set; } = inner;
    public ExceptionOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionOverride? Find(string code)
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

    public void Save(ExceptionOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception overrides. Empty fields match all.</summary>
public sealed record ExceptionOverrideQuery(string Name, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionOverride item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<ExceptionOverride> Apply(List<ExceptionOverride> items)
    {
        var matched = Collections.Filter(items, (ExceptionOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception override store.</summary>
public static class ExceptionOverrideStore
{
    public static IExceptionOverrideStore NewMemoryExceptionOverrideStore()
    {
        return new MemoryExceptionOverrideStore(new Dictionary<string, ExceptionOverride>(), new List<string>());
    }

    public static IExceptionOverrideStore NewCachedExceptionOverrideStore(MemoryExceptionOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionOverrideStore(inner, ExceptionOverrideCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ExceptionOverride> FindExceptionOverrides(IExceptionOverrideStore store, ExceptionOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
