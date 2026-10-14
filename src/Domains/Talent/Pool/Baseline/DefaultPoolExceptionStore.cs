namespace Acme.Workforce.Talent.Pool.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default pool exceptions by code.</summary>
public interface IDefaultPoolExceptionStore
{
    DefaultPoolException? Find(string code);
    void Save(DefaultPoolException item);
    bool Remove(string code);
    List<DefaultPoolException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultPoolExceptionStore(Dictionary<string, DefaultPoolException> items, List<string> order) : IDefaultPoolExceptionStore
{
    public Dictionary<string, DefaultPoolException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultPoolException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultPoolException item)
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

    public List<DefaultPoolException> List()
    {
        var outValue = new List<DefaultPoolException>();
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
public sealed class CachedDefaultPoolExceptionStore(MemoryDefaultPoolExceptionStore inner, DefaultPoolExceptionCache cache, int hits, int misses) : IDefaultPoolExceptionStore
{
    public MemoryDefaultPoolExceptionStore Inner { get; set; } = inner;
    public DefaultPoolExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultPoolException? Find(string code)
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

    public void Save(DefaultPoolException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultPoolException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default pool exceptions. Empty fields match all.</summary>
public sealed record DefaultPoolExceptionQuery(string Segment, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultPoolException item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultPoolException> Apply(List<DefaultPoolException> items)
    {
        var matched = Collections.Filter(items, (DefaultPoolException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default pool exception store.</summary>
public static class DefaultPoolExceptionStore
{
    public static IDefaultPoolExceptionStore NewMemoryDefaultPoolExceptionStore()
    {
        return new MemoryDefaultPoolExceptionStore(
            new Dictionary<string, DefaultPoolException>(),
            new List<string>());
    }

    public static IDefaultPoolExceptionStore NewCachedDefaultPoolExceptionStore(MemoryDefaultPoolExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultPoolExceptionStore(inner, DefaultPoolExceptionCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<DefaultPoolException> FindDefaultPoolExceptions(IDefaultPoolExceptionStore store, DefaultPoolExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
