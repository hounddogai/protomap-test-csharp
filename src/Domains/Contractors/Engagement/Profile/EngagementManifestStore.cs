namespace Acme.Workforce.Contractors.Engagement.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps engagement manifests by code.</summary>
public interface IEngagementManifestStore
{
    EngagementManifest? Find(string code);
    void Save(EngagementManifest item);
    bool Remove(string code);
    List<EngagementManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEngagementManifestStore(Dictionary<string, EngagementManifest> items, List<string> order) : IEngagementManifestStore
{
    public Dictionary<string, EngagementManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EngagementManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EngagementManifest item)
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

    public List<EngagementManifest> List()
    {
        var outValue = new List<EngagementManifest>();
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
public sealed class CachedEngagementManifestStore(MemoryEngagementManifestStore inner, EngagementManifestCache cache, int hits, int misses) : IEngagementManifestStore
{
    public MemoryEngagementManifestStore Inner { get; set; } = inner;
    public EngagementManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EngagementManifest? Find(string code)
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

    public void Save(EngagementManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EngagementManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects engagement manifests. Empty fields match all.</summary>
public sealed record EngagementManifestQuery(string Channel, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(EngagementManifest item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<EngagementManifest> Apply(List<EngagementManifest> items)
    {
        var matched = Collections.Filter(items, (EngagementManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of engagement manifest store.</summary>
public static class EngagementManifestStore
{
    public static IEngagementManifestStore NewMemoryEngagementManifestStore()
    {
        return new MemoryEngagementManifestStore(new Dictionary<string, EngagementManifest>(), new List<string>());
    }

    public static IEngagementManifestStore NewCachedEngagementManifestStore(MemoryEngagementManifestStore inner, int ttl, IClock clock)
    {
        return new CachedEngagementManifestStore(inner, EngagementManifestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<EngagementManifest> FindEngagementManifests(IEngagementManifestStore store, EngagementManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
