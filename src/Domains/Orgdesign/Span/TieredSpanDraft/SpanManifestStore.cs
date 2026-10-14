namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span manifests by code.</summary>
public interface ISpanManifestStore
{
    SpanManifest? Find(string code);
    void Save(SpanManifest item);
    bool Remove(string code);
    List<SpanManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanManifestStore(Dictionary<string, SpanManifest> items, List<string> order) : ISpanManifestStore
{
    public Dictionary<string, SpanManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanManifest item)
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

    public List<SpanManifest> List()
    {
        var outValue = new List<SpanManifest>();
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
public sealed class CachedSpanManifestStore(MemorySpanManifestStore inner, SpanManifestCache cache, int hits, int misses) : ISpanManifestStore
{
    public MemorySpanManifestStore Inner { get; set; } = inner;
    public SpanManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanManifest? Find(string code)
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

    public void Save(SpanManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span manifests. Empty fields match all.</summary>
public sealed record SpanManifestQuery(string Region, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(SpanManifest item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanManifest> Apply(List<SpanManifest> items)
    {
        var matched = Collections.Filter(items, (SpanManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span manifest store.</summary>
public static class SpanManifestStore
{
    public static ISpanManifestStore NewMemorySpanManifestStore()
    {
        return new MemorySpanManifestStore(new Dictionary<string, SpanManifest>(), new List<string>());
    }

    public static ISpanManifestStore NewCachedSpanManifestStore(MemorySpanManifestStore inner, int ttl, IClock clock)
    {
        return new CachedSpanManifestStore(inner, SpanManifestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<SpanManifest> FindSpanManifests(ISpanManifestStore store, SpanManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
