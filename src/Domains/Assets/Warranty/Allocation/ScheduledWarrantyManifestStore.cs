namespace Acme.Workforce.Assets.Warranty.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled warranty manifests by code.</summary>
public interface IScheduledWarrantyManifestStore
{
    ScheduledWarrantyManifest? Find(string code);
    void Save(ScheduledWarrantyManifest item);
    bool Remove(string code);
    List<ScheduledWarrantyManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledWarrantyManifestStore(Dictionary<string, ScheduledWarrantyManifest> items, List<string> order) : IScheduledWarrantyManifestStore
{
    public Dictionary<string, ScheduledWarrantyManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledWarrantyManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledWarrantyManifest item)
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

    public List<ScheduledWarrantyManifest> List()
    {
        var outValue = new List<ScheduledWarrantyManifest>();
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
public sealed class CachedScheduledWarrantyManifestStore(MemoryScheduledWarrantyManifestStore inner, ScheduledWarrantyManifestCache cache, int hits, int misses) : IScheduledWarrantyManifestStore
{
    public MemoryScheduledWarrantyManifestStore Inner { get; set; } = inner;
    public ScheduledWarrantyManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledWarrantyManifest? Find(string code)
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

    public void Save(ScheduledWarrantyManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledWarrantyManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled warranty manifests. Empty fields match all.</summary>
public sealed record ScheduledWarrantyManifestQuery(string Label, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledWarrantyManifest item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledWarrantyManifest> Apply(List<ScheduledWarrantyManifest> items)
    {
        var matched = Collections.Filter(items, (ScheduledWarrantyManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled warranty manifest store.</summary>
public static class ScheduledWarrantyManifestStore
{
    public static IScheduledWarrantyManifestStore NewMemoryScheduledWarrantyManifestStore()
    {
        return new MemoryScheduledWarrantyManifestStore(
            new Dictionary<string, ScheduledWarrantyManifest>(),
            new List<string>());
    }

    public static IScheduledWarrantyManifestStore NewCachedScheduledWarrantyManifestStore(MemoryScheduledWarrantyManifestStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledWarrantyManifestStore(
            inner,
            ScheduledWarrantyManifestCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledWarrantyManifest> FindScheduledWarrantyManifests(IScheduledWarrantyManifestStore store, ScheduledWarrantyManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
