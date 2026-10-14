namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled scenario manifests by code.</summary>
public interface IScheduledScenarioManifestStore
{
    ScheduledScenarioManifest? Find(string code);
    void Save(ScheduledScenarioManifest item);
    bool Remove(string code);
    List<ScheduledScenarioManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledScenarioManifestStore(Dictionary<string, ScheduledScenarioManifest> items, List<string> order) : IScheduledScenarioManifestStore
{
    public Dictionary<string, ScheduledScenarioManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledScenarioManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledScenarioManifest item)
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

    public List<ScheduledScenarioManifest> List()
    {
        var outValue = new List<ScheduledScenarioManifest>();
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
public sealed class CachedScheduledScenarioManifestStore(MemoryScheduledScenarioManifestStore inner, ScheduledScenarioManifestCache cache, int hits, int misses) : IScheduledScenarioManifestStore
{
    public MemoryScheduledScenarioManifestStore Inner { get; set; } = inner;
    public ScheduledScenarioManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledScenarioManifest? Find(string code)
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

    public void Save(ScheduledScenarioManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledScenarioManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled scenario manifests. Empty fields match all.</summary>
public sealed record ScheduledScenarioManifestQuery(string Name, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledScenarioManifest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledScenarioManifest> Apply(List<ScheduledScenarioManifest> items)
    {
        var matched = Collections.Filter(items, (ScheduledScenarioManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled scenario manifest store.</summary>
public static class ScheduledScenarioManifestStore
{
    public static IScheduledScenarioManifestStore NewMemoryScheduledScenarioManifestStore()
    {
        return new MemoryScheduledScenarioManifestStore(
            new Dictionary<string, ScheduledScenarioManifest>(),
            new List<string>());
    }

    public static IScheduledScenarioManifestStore NewCachedScheduledScenarioManifestStore(MemoryScheduledScenarioManifestStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledScenarioManifestStore(
            inner,
            ScheduledScenarioManifestCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledScenarioManifest> FindScheduledScenarioManifests(IScheduledScenarioManifestStore store, ScheduledScenarioManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
