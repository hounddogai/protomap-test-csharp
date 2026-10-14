namespace Acme.Workforce.Talent.Pool.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global pool exceptions by code.</summary>
public interface IGlobalPoolExceptionStore
{
    GlobalPoolException? Find(string code);
    void Save(GlobalPoolException item);
    bool Remove(string code);
    List<GlobalPoolException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalPoolExceptionStore(Dictionary<string, GlobalPoolException> items, List<string> order) : IGlobalPoolExceptionStore
{
    public Dictionary<string, GlobalPoolException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalPoolException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalPoolException item)
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

    public List<GlobalPoolException> List()
    {
        var outValue = new List<GlobalPoolException>();
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
public sealed class CachedGlobalPoolExceptionStore(MemoryGlobalPoolExceptionStore inner, GlobalPoolExceptionCache cache, int hits, int misses) : IGlobalPoolExceptionStore
{
    public MemoryGlobalPoolExceptionStore Inner { get; set; } = inner;
    public GlobalPoolExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalPoolException? Find(string code)
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

    public void Save(GlobalPoolException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalPoolException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global pool exceptions. Empty fields match all.</summary>
public sealed record GlobalPoolExceptionQuery(string Country, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalPoolException item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalPoolException> Apply(List<GlobalPoolException> items)
    {
        var matched = Collections.Filter(items, (GlobalPoolException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global pool exception store.</summary>
public static class GlobalPoolExceptionStore
{
    public static IGlobalPoolExceptionStore NewMemoryGlobalPoolExceptionStore()
    {
        return new MemoryGlobalPoolExceptionStore(new Dictionary<string, GlobalPoolException>(), new List<string>());
    }

    public static IGlobalPoolExceptionStore NewCachedGlobalPoolExceptionStore(MemoryGlobalPoolExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalPoolExceptionStore(inner, GlobalPoolExceptionCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<GlobalPoolException> FindGlobalPoolExceptions(IGlobalPoolExceptionStore store, GlobalPoolExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
