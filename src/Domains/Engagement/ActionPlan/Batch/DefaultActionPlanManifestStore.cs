namespace Acme.Workforce.Engagement.ActionPlan.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default action plan manifests by code.</summary>
public interface IDefaultActionPlanManifestStore
{
    DefaultActionPlanManifest? Find(string code);
    void Save(DefaultActionPlanManifest item);
    bool Remove(string code);
    List<DefaultActionPlanManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultActionPlanManifestStore(Dictionary<string, DefaultActionPlanManifest> items, List<string> order) : IDefaultActionPlanManifestStore
{
    public Dictionary<string, DefaultActionPlanManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultActionPlanManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultActionPlanManifest item)
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

    public List<DefaultActionPlanManifest> List()
    {
        var outValue = new List<DefaultActionPlanManifest>();
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
public sealed class CachedDefaultActionPlanManifestStore(MemoryDefaultActionPlanManifestStore inner, DefaultActionPlanManifestCache cache, int hits, int misses) : IDefaultActionPlanManifestStore
{
    public MemoryDefaultActionPlanManifestStore Inner { get; set; } = inner;
    public DefaultActionPlanManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultActionPlanManifest? Find(string code)
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

    public void Save(DefaultActionPlanManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultActionPlanManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default action plan manifests. Empty fields match all.</summary>
public sealed record DefaultActionPlanManifestQuery(string Name, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultActionPlanManifest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultActionPlanManifest> Apply(List<DefaultActionPlanManifest> items)
    {
        var matched = Collections.Filter(items, (DefaultActionPlanManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default action plan manifest store.</summary>
public static class DefaultActionPlanManifestStore
{
    public static IDefaultActionPlanManifestStore NewMemoryDefaultActionPlanManifestStore()
    {
        return new MemoryDefaultActionPlanManifestStore(
            new Dictionary<string, DefaultActionPlanManifest>(),
            new List<string>());
    }

    public static IDefaultActionPlanManifestStore NewCachedDefaultActionPlanManifestStore(MemoryDefaultActionPlanManifestStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultActionPlanManifestStore(
            inner,
            DefaultActionPlanManifestCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<DefaultActionPlanManifest> FindDefaultActionPlanManifests(IDefaultActionPlanManifestStore store, DefaultActionPlanManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
