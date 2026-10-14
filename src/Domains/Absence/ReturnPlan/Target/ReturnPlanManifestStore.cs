namespace Acme.Workforce.Absence.ReturnPlan.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan manifests by code.</summary>
public interface IReturnPlanManifestStore
{
    ReturnPlanManifest? Find(string code);
    void Save(ReturnPlanManifest item);
    bool Remove(string code);
    List<ReturnPlanManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanManifestStore(Dictionary<string, ReturnPlanManifest> items, List<string> order) : IReturnPlanManifestStore
{
    public Dictionary<string, ReturnPlanManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanManifest item)
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

    public List<ReturnPlanManifest> List()
    {
        var outValue = new List<ReturnPlanManifest>();
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
public sealed class CachedReturnPlanManifestStore(MemoryReturnPlanManifestStore inner, ReturnPlanManifestCache cache, int hits, int misses) : IReturnPlanManifestStore
{
    public MemoryReturnPlanManifestStore Inner { get; set; } = inner;
    public ReturnPlanManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanManifest? Find(string code)
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

    public void Save(ReturnPlanManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan manifests. Empty fields match all.</summary>
public sealed record ReturnPlanManifestQuery(string Team, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanManifest item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<ReturnPlanManifest> Apply(List<ReturnPlanManifest> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan manifest store.</summary>
public static class ReturnPlanManifestStore
{
    public static IReturnPlanManifestStore NewMemoryReturnPlanManifestStore()
    {
        return new MemoryReturnPlanManifestStore(new Dictionary<string, ReturnPlanManifest>(), new List<string>());
    }

    public static IReturnPlanManifestStore NewCachedReturnPlanManifestStore(MemoryReturnPlanManifestStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanManifestStore(inner, ReturnPlanManifestCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanManifest> FindReturnPlanManifests(IReturnPlanManifestStore store, ReturnPlanManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
