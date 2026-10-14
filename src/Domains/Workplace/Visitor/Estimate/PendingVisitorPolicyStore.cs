namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending visitor policies by code.</summary>
public interface IPendingVisitorPolicyStore
{
    PendingVisitorPolicy? Find(string code);
    void Save(PendingVisitorPolicy item);
    bool Remove(string code);
    List<PendingVisitorPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingVisitorPolicyStore(Dictionary<string, PendingVisitorPolicy> items, List<string> order) : IPendingVisitorPolicyStore
{
    public Dictionary<string, PendingVisitorPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingVisitorPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingVisitorPolicy item)
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

    public List<PendingVisitorPolicy> List()
    {
        var outValue = new List<PendingVisitorPolicy>();
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
public sealed class CachedPendingVisitorPolicyStore(MemoryPendingVisitorPolicyStore inner, PendingVisitorPolicyCache cache, int hits, int misses) : IPendingVisitorPolicyStore
{
    public MemoryPendingVisitorPolicyStore Inner { get; set; } = inner;
    public PendingVisitorPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingVisitorPolicy? Find(string code)
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

    public void Save(PendingVisitorPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingVisitorPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending visitor policies. Empty fields match all.</summary>
public sealed record PendingVisitorPolicyQuery(string Category, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(PendingVisitorPolicy item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingVisitorPolicy> Apply(List<PendingVisitorPolicy> items)
    {
        var matched = Collections.Filter(items, (PendingVisitorPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending visitor policy store.</summary>
public static class PendingVisitorPolicyStore
{
    public static IPendingVisitorPolicyStore NewMemoryPendingVisitorPolicyStore()
    {
        return new MemoryPendingVisitorPolicyStore(
            new Dictionary<string, PendingVisitorPolicy>(),
            new List<string>());
    }

    public static IPendingVisitorPolicyStore NewCachedPendingVisitorPolicyStore(MemoryPendingVisitorPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedPendingVisitorPolicyStore(inner, PendingVisitorPolicyCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PendingVisitorPolicy> FindPendingVisitorPolicies(IPendingVisitorPolicyStore store, PendingVisitorPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
