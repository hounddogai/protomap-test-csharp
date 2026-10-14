namespace Acme.Workforce.Compliance.Exception.ExceptionGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual exception groups by code.</summary>
public interface IManualExceptionGroupStore
{
    ManualExceptionGroup? Find(string code);
    void Save(ManualExceptionGroup item);
    bool Remove(string code);
    List<ManualExceptionGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualExceptionGroupStore(Dictionary<string, ManualExceptionGroup> items, List<string> order) : IManualExceptionGroupStore
{
    public Dictionary<string, ManualExceptionGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualExceptionGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualExceptionGroup item)
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

    public List<ManualExceptionGroup> List()
    {
        var outValue = new List<ManualExceptionGroup>();
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
public sealed class CachedManualExceptionGroupStore(MemoryManualExceptionGroupStore inner, ManualExceptionGroupCache cache, int hits, int misses) : IManualExceptionGroupStore
{
    public MemoryManualExceptionGroupStore Inner { get; set; } = inner;
    public ManualExceptionGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualExceptionGroup? Find(string code)
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

    public void Save(ManualExceptionGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualExceptionGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual exception groups. Empty fields match all.</summary>
public sealed record ManualExceptionGroupQuery(string Name, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(ManualExceptionGroup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualExceptionGroup> Apply(List<ManualExceptionGroup> items)
    {
        var matched = Collections.Filter(items, (ManualExceptionGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual exception group store.</summary>
public static class ManualExceptionGroupStore
{
    public static IManualExceptionGroupStore NewMemoryManualExceptionGroupStore()
    {
        return new MemoryManualExceptionGroupStore(
            new Dictionary<string, ManualExceptionGroup>(),
            new List<string>());
    }

    public static IManualExceptionGroupStore NewCachedManualExceptionGroupStore(MemoryManualExceptionGroupStore inner, int ttl, IClock clock)
    {
        return new CachedManualExceptionGroupStore(inner, ManualExceptionGroupCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ManualExceptionGroup> FindManualExceptionGroups(IManualExceptionGroupStore store, ManualExceptionGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
