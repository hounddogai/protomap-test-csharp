namespace Acme.Workforce.Talent.Mobility.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled mobility manifests by code.</summary>
public interface IScheduledMobilityManifestStore
{
    ScheduledMobilityManifest? Find(string code);
    void Save(ScheduledMobilityManifest item);
    bool Remove(string code);
    List<ScheduledMobilityManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledMobilityManifestStore(Dictionary<string, ScheduledMobilityManifest> items, List<string> order) : IScheduledMobilityManifestStore
{
    public Dictionary<string, ScheduledMobilityManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledMobilityManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledMobilityManifest item)
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

    public List<ScheduledMobilityManifest> List()
    {
        var outValue = new List<ScheduledMobilityManifest>();
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
public sealed class CachedScheduledMobilityManifestStore(MemoryScheduledMobilityManifestStore inner, ScheduledMobilityManifestCache cache, int hits, int misses) : IScheduledMobilityManifestStore
{
    public MemoryScheduledMobilityManifestStore Inner { get; set; } = inner;
    public ScheduledMobilityManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledMobilityManifest? Find(string code)
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

    public void Save(ScheduledMobilityManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledMobilityManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled mobility manifests. Empty fields match all.</summary>
public sealed record ScheduledMobilityManifestQuery(string Locale, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledMobilityManifest item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<ScheduledMobilityManifest> Apply(List<ScheduledMobilityManifest> items)
    {
        var matched = Collections.Filter(items, (ScheduledMobilityManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled mobility manifest store.</summary>
public static class ScheduledMobilityManifestStore
{
    public static IScheduledMobilityManifestStore NewMemoryScheduledMobilityManifestStore()
    {
        return new MemoryScheduledMobilityManifestStore(
            new Dictionary<string, ScheduledMobilityManifest>(),
            new List<string>());
    }

    public static IScheduledMobilityManifestStore NewCachedScheduledMobilityManifestStore(MemoryScheduledMobilityManifestStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledMobilityManifestStore(
            inner,
            ScheduledMobilityManifestCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledMobilityManifest> FindScheduledMobilityManifests(IScheduledMobilityManifestStore store, ScheduledMobilityManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
