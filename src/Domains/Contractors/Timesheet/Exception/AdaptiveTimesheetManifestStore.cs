namespace Acme.Workforce.Contractors.Timesheet.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive timesheet manifests by code.</summary>
public interface IAdaptiveTimesheetManifestStore
{
    AdaptiveTimesheetManifest? Find(string code);
    void Save(AdaptiveTimesheetManifest item);
    bool Remove(string code);
    List<AdaptiveTimesheetManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveTimesheetManifestStore(Dictionary<string, AdaptiveTimesheetManifest> items, List<string> order) : IAdaptiveTimesheetManifestStore
{
    public Dictionary<string, AdaptiveTimesheetManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveTimesheetManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveTimesheetManifest item)
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

    public List<AdaptiveTimesheetManifest> List()
    {
        var outValue = new List<AdaptiveTimesheetManifest>();
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
public sealed class CachedAdaptiveTimesheetManifestStore(MemoryAdaptiveTimesheetManifestStore inner, AdaptiveTimesheetManifestCache cache, int hits, int misses) : IAdaptiveTimesheetManifestStore
{
    public MemoryAdaptiveTimesheetManifestStore Inner { get; set; } = inner;
    public AdaptiveTimesheetManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveTimesheetManifest? Find(string code)
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

    public void Save(AdaptiveTimesheetManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveTimesheetManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive timesheet manifests. Empty fields match all.</summary>
public sealed record AdaptiveTimesheetManifestQuery(string Label, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveTimesheetManifest item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveTimesheetManifest> Apply(List<AdaptiveTimesheetManifest> items)
    {
        var matched = Collections.Filter(items, (AdaptiveTimesheetManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive timesheet manifest store.</summary>
public static class AdaptiveTimesheetManifestStore
{
    public static IAdaptiveTimesheetManifestStore NewMemoryAdaptiveTimesheetManifestStore()
    {
        return new MemoryAdaptiveTimesheetManifestStore(
            new Dictionary<string, AdaptiveTimesheetManifest>(),
            new List<string>());
    }

    public static IAdaptiveTimesheetManifestStore NewCachedAdaptiveTimesheetManifestStore(MemoryAdaptiveTimesheetManifestStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveTimesheetManifestStore(
            inner,
            AdaptiveTimesheetManifestCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveTimesheetManifest> FindAdaptiveTimesheetManifests(IAdaptiveTimesheetManifestStore store, AdaptiveTimesheetManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
