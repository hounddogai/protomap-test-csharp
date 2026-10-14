namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception claims by code.</summary>
public interface IExceptionClaimStore
{
    ExceptionClaim? Find(string code);
    void Save(ExceptionClaim item);
    bool Remove(string code);
    List<ExceptionClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionClaimStore(Dictionary<string, ExceptionClaim> items, List<string> order) : IExceptionClaimStore
{
    public Dictionary<string, ExceptionClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionClaim item)
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

    public List<ExceptionClaim> List()
    {
        var outValue = new List<ExceptionClaim>();
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
public sealed class CachedExceptionClaimStore(MemoryExceptionClaimStore inner, ExceptionClaimCache cache, int hits, int misses) : IExceptionClaimStore
{
    public MemoryExceptionClaimStore Inner { get; set; } = inner;
    public ExceptionClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionClaim? Find(string code)
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

    public void Save(ExceptionClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception claims. Empty fields match all.</summary>
public sealed record ExceptionClaimQuery(string Channel, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionClaim item)
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

    public List<ExceptionClaim> Apply(List<ExceptionClaim> items)
    {
        var matched = Collections.Filter(items, (ExceptionClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception claim store.</summary>
public static class ExceptionClaimStore
{
    public static IExceptionClaimStore NewMemoryExceptionClaimStore()
    {
        return new MemoryExceptionClaimStore(new Dictionary<string, ExceptionClaim>(), new List<string>());
    }

    public static IExceptionClaimStore NewCachedExceptionClaimStore(MemoryExceptionClaimStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionClaimStore(inner, ExceptionClaimCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ExceptionClaim> FindExceptionClaims(IExceptionClaimStore store, ExceptionClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
