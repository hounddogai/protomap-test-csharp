namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global scenario requests by code.</summary>
public interface IGlobalScenarioRequestStore
{
    GlobalScenarioRequest? Find(string code);
    void Save(GlobalScenarioRequest item);
    bool Remove(string code);
    List<GlobalScenarioRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalScenarioRequestStore(Dictionary<string, GlobalScenarioRequest> items, List<string> order) : IGlobalScenarioRequestStore
{
    public Dictionary<string, GlobalScenarioRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalScenarioRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalScenarioRequest item)
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

    public List<GlobalScenarioRequest> List()
    {
        var outValue = new List<GlobalScenarioRequest>();
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
public sealed class CachedGlobalScenarioRequestStore(MemoryGlobalScenarioRequestStore inner, GlobalScenarioRequestCache cache, int hits, int misses) : IGlobalScenarioRequestStore
{
    public MemoryGlobalScenarioRequestStore Inner { get; set; } = inner;
    public GlobalScenarioRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalScenarioRequest? Find(string code)
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

    public void Save(GlobalScenarioRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalScenarioRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global scenario requests. Empty fields match all.</summary>
public sealed record GlobalScenarioRequestQuery(string Source, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalScenarioRequest item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalScenarioRequest> Apply(List<GlobalScenarioRequest> items)
    {
        var matched = Collections.Filter(items, (GlobalScenarioRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global scenario request store.</summary>
public static class GlobalScenarioRequestStore
{
    public static IGlobalScenarioRequestStore NewMemoryGlobalScenarioRequestStore()
    {
        return new MemoryGlobalScenarioRequestStore(
            new Dictionary<string, GlobalScenarioRequest>(),
            new List<string>());
    }

    public static IGlobalScenarioRequestStore NewCachedGlobalScenarioRequestStore(MemoryGlobalScenarioRequestStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalScenarioRequestStore(inner, GlobalScenarioRequestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<GlobalScenarioRequest> FindGlobalScenarioRequests(IGlobalScenarioRequestStore store, GlobalScenarioRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
