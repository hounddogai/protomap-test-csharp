namespace Acme.Workforce.Absence.ReturnPlan.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default return plan manifests by code.</summary>
public interface IDefaultReturnPlanManifestStore
{
    DefaultReturnPlanManifest? Find(string code);
    void Save(DefaultReturnPlanManifest item);
    bool Remove(string code);
    List<DefaultReturnPlanManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultReturnPlanManifestStore(Dictionary<string, DefaultReturnPlanManifest> items, List<string> order) : IDefaultReturnPlanManifestStore
{
    public Dictionary<string, DefaultReturnPlanManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultReturnPlanManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultReturnPlanManifest item)
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

    public List<DefaultReturnPlanManifest> List()
    {
        var outValue = new List<DefaultReturnPlanManifest>();
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
public sealed class CachedDefaultReturnPlanManifestStore(MemoryDefaultReturnPlanManifestStore inner, DefaultReturnPlanManifestCache cache, int hits, int misses) : IDefaultReturnPlanManifestStore
{
    public MemoryDefaultReturnPlanManifestStore Inner { get; set; } = inner;
    public DefaultReturnPlanManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultReturnPlanManifest? Find(string code)
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

    public void Save(DefaultReturnPlanManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultReturnPlanManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default return plan manifests. Empty fields match all.</summary>
public sealed record DefaultReturnPlanManifestQuery(string Team, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultReturnPlanManifest item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultReturnPlanManifest> Apply(List<DefaultReturnPlanManifest> items)
    {
        var matched = Collections.Filter(items, (DefaultReturnPlanManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default return plan manifest store.</summary>
public static class DefaultReturnPlanManifestStore
{
    public static IDefaultReturnPlanManifestStore NewMemoryDefaultReturnPlanManifestStore()
    {
        return new MemoryDefaultReturnPlanManifestStore(
            new Dictionary<string, DefaultReturnPlanManifest>(),
            new List<string>());
    }

    public static IDefaultReturnPlanManifestStore NewCachedDefaultReturnPlanManifestStore(MemoryDefaultReturnPlanManifestStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultReturnPlanManifestStore(
            inner,
            DefaultReturnPlanManifestCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<DefaultReturnPlanManifest> FindDefaultReturnPlanManifests(IDefaultReturnPlanManifestStore store, DefaultReturnPlanManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
