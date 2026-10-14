namespace Acme.Workforce.Workplace.Visitor.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor bundles by code.</summary>
public interface IVisitorBundleStore
{
    VisitorBundle? Find(string code);
    void Save(VisitorBundle item);
    bool Remove(string code);
    List<VisitorBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorBundleStore(Dictionary<string, VisitorBundle> items, List<string> order) : IVisitorBundleStore
{
    public Dictionary<string, VisitorBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorBundle item)
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

    public List<VisitorBundle> List()
    {
        var outValue = new List<VisitorBundle>();
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
public sealed class CachedVisitorBundleStore(MemoryVisitorBundleStore inner, VisitorBundleCache cache, int hits, int misses) : IVisitorBundleStore
{
    public MemoryVisitorBundleStore Inner { get; set; } = inner;
    public VisitorBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorBundle? Find(string code)
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

    public void Save(VisitorBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor bundles. Empty fields match all.</summary>
public sealed record VisitorBundleQuery(string Name, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorBundle item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorBundle> Apply(List<VisitorBundle> items)
    {
        var matched = Collections.Filter(items, (VisitorBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor bundle store.</summary>
public static class VisitorBundleStore
{
    public static IVisitorBundleStore NewMemoryVisitorBundleStore()
    {
        return new MemoryVisitorBundleStore(new Dictionary<string, VisitorBundle>(), new List<string>());
    }

    public static IVisitorBundleStore NewCachedVisitorBundleStore(MemoryVisitorBundleStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorBundleStore(inner, VisitorBundleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<VisitorBundle> FindVisitorBundles(IVisitorBundleStore store, VisitorBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
