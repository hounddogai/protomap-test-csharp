namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount requests by code.</summary>
public interface IHeadcountRequestStore
{
    HeadcountRequest? Find(string code);
    void Save(HeadcountRequest item);
    bool Remove(string code);
    List<HeadcountRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountRequestStore(Dictionary<string, HeadcountRequest> items, List<string> order) : IHeadcountRequestStore
{
    public Dictionary<string, HeadcountRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountRequest item)
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

    public List<HeadcountRequest> List()
    {
        var outValue = new List<HeadcountRequest>();
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
public sealed class CachedHeadcountRequestStore(MemoryHeadcountRequestStore inner, HeadcountRequestCache cache, int hits, int misses) : IHeadcountRequestStore
{
    public MemoryHeadcountRequestStore Inner { get; set; } = inner;
    public HeadcountRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountRequest? Find(string code)
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

    public void Save(HeadcountRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount requests. Empty fields match all.</summary>
public sealed record HeadcountRequestQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountRequest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountRequest> Apply(List<HeadcountRequest> items)
    {
        var matched = Collections.Filter(items, (HeadcountRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount request store.</summary>
public static class HeadcountRequestStore
{
    public static IHeadcountRequestStore NewMemoryHeadcountRequestStore()
    {
        return new MemoryHeadcountRequestStore(new Dictionary<string, HeadcountRequest>(), new List<string>());
    }

    public static IHeadcountRequestStore NewCachedHeadcountRequestStore(MemoryHeadcountRequestStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountRequestStore(inner, HeadcountRequestCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<HeadcountRequest> FindHeadcountRequests(IHeadcountRequestStore store, HeadcountRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
