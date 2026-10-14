namespace Acme.Workforce.Workplace.AccessBadge.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge exceptions by code.</summary>
public interface IAccessBadgeExceptionStore
{
    AccessBadgeException? Find(string code);
    void Save(AccessBadgeException item);
    bool Remove(string code);
    List<AccessBadgeException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeExceptionStore(Dictionary<string, AccessBadgeException> items, List<string> order) : IAccessBadgeExceptionStore
{
    public Dictionary<string, AccessBadgeException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeException item)
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

    public List<AccessBadgeException> List()
    {
        var outValue = new List<AccessBadgeException>();
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
public sealed class CachedAccessBadgeExceptionStore(MemoryAccessBadgeExceptionStore inner, AccessBadgeExceptionCache cache, int hits, int misses) : IAccessBadgeExceptionStore
{
    public MemoryAccessBadgeExceptionStore Inner { get; set; } = inner;
    public AccessBadgeExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeException? Find(string code)
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

    public void Save(AccessBadgeException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge exceptions. Empty fields match all.</summary>
public sealed record AccessBadgeExceptionQuery(string Title, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeException item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccessBadgeException> Apply(List<AccessBadgeException> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge exception store.</summary>
public static class AccessBadgeExceptionStore
{
    public static IAccessBadgeExceptionStore NewMemoryAccessBadgeExceptionStore()
    {
        return new MemoryAccessBadgeExceptionStore(
            new Dictionary<string, AccessBadgeException>(),
            new List<string>());
    }

    public static IAccessBadgeExceptionStore NewCachedAccessBadgeExceptionStore(MemoryAccessBadgeExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeExceptionStore(inner, AccessBadgeExceptionCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeException> FindAccessBadgeExceptions(IAccessBadgeExceptionStore store, AccessBadgeExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
