namespace Acme.Workforce.Compliance.Exception.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary exception requests by code.</summary>
public interface IPrimaryExceptionRequestStore
{
    PrimaryExceptionRequest? Find(string code);
    void Save(PrimaryExceptionRequest item);
    bool Remove(string code);
    List<PrimaryExceptionRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryExceptionRequestStore(Dictionary<string, PrimaryExceptionRequest> items, List<string> order) : IPrimaryExceptionRequestStore
{
    public Dictionary<string, PrimaryExceptionRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryExceptionRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryExceptionRequest item)
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

    public List<PrimaryExceptionRequest> List()
    {
        var outValue = new List<PrimaryExceptionRequest>();
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
public sealed class CachedPrimaryExceptionRequestStore(MemoryPrimaryExceptionRequestStore inner, PrimaryExceptionRequestCache cache, int hits, int misses) : IPrimaryExceptionRequestStore
{
    public MemoryPrimaryExceptionRequestStore Inner { get; set; } = inner;
    public PrimaryExceptionRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryExceptionRequest? Find(string code)
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

    public void Save(PrimaryExceptionRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryExceptionRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary exception requests. Empty fields match all.</summary>
public sealed record PrimaryExceptionRequestQuery(string Title, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryExceptionRequest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryExceptionRequest> Apply(List<PrimaryExceptionRequest> items)
    {
        var matched = Collections.Filter(items, (PrimaryExceptionRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary exception request store.</summary>
public static class PrimaryExceptionRequestStore
{
    public static IPrimaryExceptionRequestStore NewMemoryPrimaryExceptionRequestStore()
    {
        return new MemoryPrimaryExceptionRequestStore(
            new Dictionary<string, PrimaryExceptionRequest>(),
            new List<string>());
    }

    public static IPrimaryExceptionRequestStore NewCachedPrimaryExceptionRequestStore(MemoryPrimaryExceptionRequestStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryExceptionRequestStore(
            inner,
            PrimaryExceptionRequestCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryExceptionRequest> FindPrimaryExceptionRequests(IPrimaryExceptionRequestStore store, PrimaryExceptionRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
