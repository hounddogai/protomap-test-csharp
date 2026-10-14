namespace Acme.Workforce.Contractors.Engagement.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient engagement manifests by code.</summary>
public interface ILenientEngagementManifestStore
{
    LenientEngagementManifest? Find(string code);
    void Save(LenientEngagementManifest item);
    bool Remove(string code);
    List<LenientEngagementManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientEngagementManifestStore(Dictionary<string, LenientEngagementManifest> items, List<string> order) : ILenientEngagementManifestStore
{
    public Dictionary<string, LenientEngagementManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientEngagementManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientEngagementManifest item)
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

    public List<LenientEngagementManifest> List()
    {
        var outValue = new List<LenientEngagementManifest>();
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
public sealed class CachedLenientEngagementManifestStore(MemoryLenientEngagementManifestStore inner, LenientEngagementManifestCache cache, int hits, int misses) : ILenientEngagementManifestStore
{
    public MemoryLenientEngagementManifestStore Inner { get; set; } = inner;
    public LenientEngagementManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientEngagementManifest? Find(string code)
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

    public void Save(LenientEngagementManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientEngagementManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient engagement manifests. Empty fields match all.</summary>
public sealed record LenientEngagementManifestQuery(string Channel, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(LenientEngagementManifest item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientEngagementManifest> Apply(List<LenientEngagementManifest> items)
    {
        var matched = Collections.Filter(items, (LenientEngagementManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient engagement manifest store.</summary>
public static class LenientEngagementManifestStore
{
    public static ILenientEngagementManifestStore NewMemoryLenientEngagementManifestStore()
    {
        return new MemoryLenientEngagementManifestStore(
            new Dictionary<string, LenientEngagementManifest>(),
            new List<string>());
    }

    public static ILenientEngagementManifestStore NewCachedLenientEngagementManifestStore(MemoryLenientEngagementManifestStore inner, int ttl, IClock clock)
    {
        return new CachedLenientEngagementManifestStore(
            inner,
            LenientEngagementManifestCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LenientEngagementManifest> FindLenientEngagementManifests(ILenientEngagementManifestStore store, LenientEngagementManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
