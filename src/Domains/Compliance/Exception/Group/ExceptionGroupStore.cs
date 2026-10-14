namespace Acme.Workforce.Compliance.Exception.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception groups by code.</summary>
public interface IExceptionGroupStore
{
    ExceptionGroup? Find(string code);
    void Save(ExceptionGroup item);
    bool Remove(string code);
    List<ExceptionGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionGroupStore(Dictionary<string, ExceptionGroup> items, List<string> order) : IExceptionGroupStore
{
    public Dictionary<string, ExceptionGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionGroup item)
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

    public List<ExceptionGroup> List()
    {
        var outValue = new List<ExceptionGroup>();
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
public sealed class CachedExceptionGroupStore(MemoryExceptionGroupStore inner, ExceptionGroupCache cache, int hits, int misses) : IExceptionGroupStore
{
    public MemoryExceptionGroupStore Inner { get; set; } = inner;
    public ExceptionGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionGroup? Find(string code)
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

    public void Save(ExceptionGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception groups. Empty fields match all.</summary>
public sealed record ExceptionGroupQuery(string Channel, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionGroup item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ExceptionGroup> Apply(List<ExceptionGroup> items)
    {
        var matched = Collections.Filter(items, (ExceptionGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception group store.</summary>
public static class ExceptionGroupStore
{
    public static IExceptionGroupStore NewMemoryExceptionGroupStore()
    {
        return new MemoryExceptionGroupStore(new Dictionary<string, ExceptionGroup>(), new List<string>());
    }

    public static IExceptionGroupStore NewCachedExceptionGroupStore(MemoryExceptionGroupStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionGroupStore(inner, ExceptionGroupCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ExceptionGroup> FindExceptionGroups(IExceptionGroupStore store, ExceptionGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
