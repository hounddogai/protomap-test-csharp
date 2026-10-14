namespace Acme.Workforce.Workplace.AccessBadge.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default access badge manifests by code.</summary>
public interface IDefaultAccessBadgeManifestStore
{
    DefaultAccessBadgeManifest? Find(string code);
    void Save(DefaultAccessBadgeManifest item);
    bool Remove(string code);
    List<DefaultAccessBadgeManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultAccessBadgeManifestStore(Dictionary<string, DefaultAccessBadgeManifest> items, List<string> order) : IDefaultAccessBadgeManifestStore
{
    public Dictionary<string, DefaultAccessBadgeManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultAccessBadgeManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultAccessBadgeManifest item)
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

    public List<DefaultAccessBadgeManifest> List()
    {
        var outValue = new List<DefaultAccessBadgeManifest>();
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
public sealed class CachedDefaultAccessBadgeManifestStore(MemoryDefaultAccessBadgeManifestStore inner, DefaultAccessBadgeManifestCache cache, int hits, int misses) : IDefaultAccessBadgeManifestStore
{
    public MemoryDefaultAccessBadgeManifestStore Inner { get; set; } = inner;
    public DefaultAccessBadgeManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultAccessBadgeManifest? Find(string code)
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

    public void Save(DefaultAccessBadgeManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultAccessBadgeManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default access badge manifests. Empty fields match all.</summary>
public sealed record DefaultAccessBadgeManifestQuery(string Segment, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultAccessBadgeManifest item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultAccessBadgeManifest> Apply(List<DefaultAccessBadgeManifest> items)
    {
        var matched = Collections.Filter(items, (DefaultAccessBadgeManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default access badge manifest store.</summary>
public static class DefaultAccessBadgeManifestStore
{
    public static IDefaultAccessBadgeManifestStore NewMemoryDefaultAccessBadgeManifestStore()
    {
        return new MemoryDefaultAccessBadgeManifestStore(
            new Dictionary<string, DefaultAccessBadgeManifest>(),
            new List<string>());
    }

    public static IDefaultAccessBadgeManifestStore NewCachedDefaultAccessBadgeManifestStore(MemoryDefaultAccessBadgeManifestStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultAccessBadgeManifestStore(
            inner,
            DefaultAccessBadgeManifestCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<DefaultAccessBadgeManifest> FindDefaultAccessBadgeManifests(IDefaultAccessBadgeManifestStore store, DefaultAccessBadgeManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
