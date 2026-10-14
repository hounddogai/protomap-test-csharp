namespace Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary succession plan requests by code.</summary>
public interface IPrimarySuccessionPlanRequestStore
{
    PrimarySuccessionPlanRequest? Find(string code);
    void Save(PrimarySuccessionPlanRequest item);
    bool Remove(string code);
    List<PrimarySuccessionPlanRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimarySuccessionPlanRequestStore(Dictionary<string, PrimarySuccessionPlanRequest> items, List<string> order) : IPrimarySuccessionPlanRequestStore
{
    public Dictionary<string, PrimarySuccessionPlanRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimarySuccessionPlanRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimarySuccessionPlanRequest item)
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

    public List<PrimarySuccessionPlanRequest> List()
    {
        var outValue = new List<PrimarySuccessionPlanRequest>();
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
public sealed class CachedPrimarySuccessionPlanRequestStore(MemoryPrimarySuccessionPlanRequestStore inner, PrimarySuccessionPlanRequestCache cache, int hits, int misses) : IPrimarySuccessionPlanRequestStore
{
    public MemoryPrimarySuccessionPlanRequestStore Inner { get; set; } = inner;
    public PrimarySuccessionPlanRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimarySuccessionPlanRequest? Find(string code)
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

    public void Save(PrimarySuccessionPlanRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimarySuccessionPlanRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary succession plan requests. Empty fields match all.</summary>
public sealed record PrimarySuccessionPlanRequestQuery(string Title, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(PrimarySuccessionPlanRequest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimarySuccessionPlanRequest> Apply(List<PrimarySuccessionPlanRequest> items)
    {
        var matched = Collections.Filter(items, (PrimarySuccessionPlanRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary succession plan request store.</summary>
public static class PrimarySuccessionPlanRequestStore
{
    public static IPrimarySuccessionPlanRequestStore NewMemoryPrimarySuccessionPlanRequestStore()
    {
        return new MemoryPrimarySuccessionPlanRequestStore(
            new Dictionary<string, PrimarySuccessionPlanRequest>(),
            new List<string>());
    }

    public static IPrimarySuccessionPlanRequestStore NewCachedPrimarySuccessionPlanRequestStore(MemoryPrimarySuccessionPlanRequestStore inner, int ttl, IClock clock)
    {
        return new CachedPrimarySuccessionPlanRequestStore(
            inner,
            PrimarySuccessionPlanRequestCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<PrimarySuccessionPlanRequest> FindPrimarySuccessionPlanRequests(IPrimarySuccessionPlanRequestStore store, PrimarySuccessionPlanRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
