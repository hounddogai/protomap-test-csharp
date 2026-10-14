namespace Acme.Workforce.Talent.SuccessionPlan.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global succession plan requests by code.</summary>
public interface IGlobalSuccessionPlanRequestStore
{
    GlobalSuccessionPlanRequest? Find(string code);
    void Save(GlobalSuccessionPlanRequest item);
    bool Remove(string code);
    List<GlobalSuccessionPlanRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalSuccessionPlanRequestStore(Dictionary<string, GlobalSuccessionPlanRequest> items, List<string> order) : IGlobalSuccessionPlanRequestStore
{
    public Dictionary<string, GlobalSuccessionPlanRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalSuccessionPlanRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalSuccessionPlanRequest item)
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

    public List<GlobalSuccessionPlanRequest> List()
    {
        var outValue = new List<GlobalSuccessionPlanRequest>();
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
public sealed class CachedGlobalSuccessionPlanRequestStore(MemoryGlobalSuccessionPlanRequestStore inner, GlobalSuccessionPlanRequestCache cache, int hits, int misses) : IGlobalSuccessionPlanRequestStore
{
    public MemoryGlobalSuccessionPlanRequestStore Inner { get; set; } = inner;
    public GlobalSuccessionPlanRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalSuccessionPlanRequest? Find(string code)
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

    public void Save(GlobalSuccessionPlanRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalSuccessionPlanRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global succession plan requests. Empty fields match all.</summary>
public sealed record GlobalSuccessionPlanRequestQuery(string Unit, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalSuccessionPlanRequest item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalSuccessionPlanRequest> Apply(List<GlobalSuccessionPlanRequest> items)
    {
        var matched = Collections.Filter(items, (GlobalSuccessionPlanRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global succession plan request store.</summary>
public static class GlobalSuccessionPlanRequestStore
{
    public static IGlobalSuccessionPlanRequestStore NewMemoryGlobalSuccessionPlanRequestStore()
    {
        return new MemoryGlobalSuccessionPlanRequestStore(
            new Dictionary<string, GlobalSuccessionPlanRequest>(),
            new List<string>());
    }

    public static IGlobalSuccessionPlanRequestStore NewCachedGlobalSuccessionPlanRequestStore(MemoryGlobalSuccessionPlanRequestStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalSuccessionPlanRequestStore(
            inner,
            GlobalSuccessionPlanRequestCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<GlobalSuccessionPlanRequest> FindGlobalSuccessionPlanRequests(IGlobalSuccessionPlanRequestStore store, GlobalSuccessionPlanRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
