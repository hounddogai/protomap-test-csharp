namespace Acme.Workforce.Talent.SuccessionPlan.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling succession plan requests by code.</summary>
public interface IRollingSuccessionPlanRequestStore
{
    RollingSuccessionPlanRequest? Find(string code);
    void Save(RollingSuccessionPlanRequest item);
    bool Remove(string code);
    List<RollingSuccessionPlanRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingSuccessionPlanRequestStore(Dictionary<string, RollingSuccessionPlanRequest> items, List<string> order) : IRollingSuccessionPlanRequestStore
{
    public Dictionary<string, RollingSuccessionPlanRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingSuccessionPlanRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingSuccessionPlanRequest item)
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

    public List<RollingSuccessionPlanRequest> List()
    {
        var outValue = new List<RollingSuccessionPlanRequest>();
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
public sealed class CachedRollingSuccessionPlanRequestStore(MemoryRollingSuccessionPlanRequestStore inner, RollingSuccessionPlanRequestCache cache, int hits, int misses) : IRollingSuccessionPlanRequestStore
{
    public MemoryRollingSuccessionPlanRequestStore Inner { get; set; } = inner;
    public RollingSuccessionPlanRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingSuccessionPlanRequest? Find(string code)
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

    public void Save(RollingSuccessionPlanRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingSuccessionPlanRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling succession plan requests. Empty fields match all.</summary>
public sealed record RollingSuccessionPlanRequestQuery(string Locale, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(RollingSuccessionPlanRequest item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<RollingSuccessionPlanRequest> Apply(List<RollingSuccessionPlanRequest> items)
    {
        var matched = Collections.Filter(items, (RollingSuccessionPlanRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling succession plan request store.</summary>
public static class RollingSuccessionPlanRequestStore
{
    public static IRollingSuccessionPlanRequestStore NewMemoryRollingSuccessionPlanRequestStore()
    {
        return new MemoryRollingSuccessionPlanRequestStore(
            new Dictionary<string, RollingSuccessionPlanRequest>(),
            new List<string>());
    }

    public static IRollingSuccessionPlanRequestStore NewCachedRollingSuccessionPlanRequestStore(MemoryRollingSuccessionPlanRequestStore inner, int ttl, IClock clock)
    {
        return new CachedRollingSuccessionPlanRequestStore(
            inner,
            RollingSuccessionPlanRequestCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RollingSuccessionPlanRequest> FindRollingSuccessionPlanRequests(IRollingSuccessionPlanRequestStore store, RollingSuccessionPlanRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
