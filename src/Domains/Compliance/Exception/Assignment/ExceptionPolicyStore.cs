namespace Acme.Workforce.Compliance.Exception.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception policies by code.</summary>
public interface IExceptionPolicyStore
{
    ExceptionPolicy? Find(string code);
    void Save(ExceptionPolicy item);
    bool Remove(string code);
    List<ExceptionPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionPolicyStore(Dictionary<string, ExceptionPolicy> items, List<string> order) : IExceptionPolicyStore
{
    public Dictionary<string, ExceptionPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionPolicy item)
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

    public List<ExceptionPolicy> List()
    {
        var outValue = new List<ExceptionPolicy>();
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
public sealed class CachedExceptionPolicyStore(MemoryExceptionPolicyStore inner, ExceptionPolicyCache cache, int hits, int misses) : IExceptionPolicyStore
{
    public MemoryExceptionPolicyStore Inner { get; set; } = inner;
    public ExceptionPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionPolicy? Find(string code)
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

    public void Save(ExceptionPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception policies. Empty fields match all.</summary>
public sealed record ExceptionPolicyQuery(string Name, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionPolicy item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ExceptionPolicy> Apply(List<ExceptionPolicy> items)
    {
        var matched = Collections.Filter(items, (ExceptionPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception policy store.</summary>
public static class ExceptionPolicyStore
{
    public static IExceptionPolicyStore NewMemoryExceptionPolicyStore()
    {
        return new MemoryExceptionPolicyStore(new Dictionary<string, ExceptionPolicy>(), new List<string>());
    }

    public static IExceptionPolicyStore NewCachedExceptionPolicyStore(MemoryExceptionPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionPolicyStore(inner, ExceptionPolicyCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ExceptionPolicy> FindExceptionPolicies(IExceptionPolicyStore store, ExceptionPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
