namespace Acme.Workforce.Talent.Pool.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered pool exceptions by code.</summary>
public interface ITieredPoolExceptionStore
{
    TieredPoolException? Find(string code);
    void Save(TieredPoolException item);
    bool Remove(string code);
    List<TieredPoolException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredPoolExceptionStore(Dictionary<string, TieredPoolException> items, List<string> order) : ITieredPoolExceptionStore
{
    public Dictionary<string, TieredPoolException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredPoolException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredPoolException item)
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

    public List<TieredPoolException> List()
    {
        var outValue = new List<TieredPoolException>();
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
public sealed class CachedTieredPoolExceptionStore(MemoryTieredPoolExceptionStore inner, TieredPoolExceptionCache cache, int hits, int misses) : ITieredPoolExceptionStore
{
    public MemoryTieredPoolExceptionStore Inner { get; set; } = inner;
    public TieredPoolExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredPoolException? Find(string code)
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

    public void Save(TieredPoolException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredPoolException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered pool exceptions. Empty fields match all.</summary>
public sealed record TieredPoolExceptionQuery(string Department, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(TieredPoolException item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredPoolException> Apply(List<TieredPoolException> items)
    {
        var matched = Collections.Filter(items, (TieredPoolException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered pool exception store.</summary>
public static class TieredPoolExceptionStore
{
    public static ITieredPoolExceptionStore NewMemoryTieredPoolExceptionStore()
    {
        return new MemoryTieredPoolExceptionStore(new Dictionary<string, TieredPoolException>(), new List<string>());
    }

    public static ITieredPoolExceptionStore NewCachedTieredPoolExceptionStore(MemoryTieredPoolExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedTieredPoolExceptionStore(inner, TieredPoolExceptionCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TieredPoolException> FindTieredPoolExceptions(ITieredPoolExceptionStore store, TieredPoolExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
