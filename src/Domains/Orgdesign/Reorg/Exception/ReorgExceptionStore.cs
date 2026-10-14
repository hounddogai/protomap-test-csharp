namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg exceptions by code.</summary>
public interface IReorgExceptionStore
{
    ReorgException? Find(string code);
    void Save(ReorgException item);
    bool Remove(string code);
    List<ReorgException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgExceptionStore(Dictionary<string, ReorgException> items, List<string> order) : IReorgExceptionStore
{
    public Dictionary<string, ReorgException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgException item)
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

    public List<ReorgException> List()
    {
        var outValue = new List<ReorgException>();
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
public sealed class CachedReorgExceptionStore(MemoryReorgExceptionStore inner, ReorgExceptionCache cache, int hits, int misses) : IReorgExceptionStore
{
    public MemoryReorgExceptionStore Inner { get; set; } = inner;
    public ReorgExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgException? Find(string code)
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

    public void Save(ReorgException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg exceptions. Empty fields match all.</summary>
public sealed record ReorgExceptionQuery(string Department, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgException item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReorgException> Apply(List<ReorgException> items)
    {
        var matched = Collections.Filter(items, (ReorgException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg exception store.</summary>
public static class ReorgExceptionStore
{
    public static IReorgExceptionStore NewMemoryReorgExceptionStore()
    {
        return new MemoryReorgExceptionStore(new Dictionary<string, ReorgException>(), new List<string>());
    }

    public static IReorgExceptionStore NewCachedReorgExceptionStore(MemoryReorgExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedReorgExceptionStore(inner, ReorgExceptionCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ReorgException> FindReorgExceptions(IReorgExceptionStore store, ReorgExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
