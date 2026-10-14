namespace Acme.Workforce.Talent.SuccessionPlan.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient succession plan requests by code.</summary>
public interface ILenientSuccessionPlanRequestStore
{
    LenientSuccessionPlanRequest? Find(string code);
    void Save(LenientSuccessionPlanRequest item);
    bool Remove(string code);
    List<LenientSuccessionPlanRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientSuccessionPlanRequestStore(Dictionary<string, LenientSuccessionPlanRequest> items, List<string> order) : ILenientSuccessionPlanRequestStore
{
    public Dictionary<string, LenientSuccessionPlanRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientSuccessionPlanRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientSuccessionPlanRequest item)
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

    public List<LenientSuccessionPlanRequest> List()
    {
        var outValue = new List<LenientSuccessionPlanRequest>();
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
public sealed class CachedLenientSuccessionPlanRequestStore(MemoryLenientSuccessionPlanRequestStore inner, LenientSuccessionPlanRequestCache cache, int hits, int misses) : ILenientSuccessionPlanRequestStore
{
    public MemoryLenientSuccessionPlanRequestStore Inner { get; set; } = inner;
    public LenientSuccessionPlanRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientSuccessionPlanRequest? Find(string code)
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

    public void Save(LenientSuccessionPlanRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientSuccessionPlanRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient succession plan requests. Empty fields match all.</summary>
public sealed record LenientSuccessionPlanRequestQuery(string Title, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(LenientSuccessionPlanRequest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientSuccessionPlanRequest> Apply(List<LenientSuccessionPlanRequest> items)
    {
        var matched = Collections.Filter(items, (LenientSuccessionPlanRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient succession plan request store.</summary>
public static class LenientSuccessionPlanRequestStore
{
    public static ILenientSuccessionPlanRequestStore NewMemoryLenientSuccessionPlanRequestStore()
    {
        return new MemoryLenientSuccessionPlanRequestStore(
            new Dictionary<string, LenientSuccessionPlanRequest>(),
            new List<string>());
    }

    public static ILenientSuccessionPlanRequestStore NewCachedLenientSuccessionPlanRequestStore(MemoryLenientSuccessionPlanRequestStore inner, int ttl, IClock clock)
    {
        return new CachedLenientSuccessionPlanRequestStore(
            inner,
            LenientSuccessionPlanRequestCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LenientSuccessionPlanRequest> FindLenientSuccessionPlanRequests(ILenientSuccessionPlanRequestStore store, LenientSuccessionPlanRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
