namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal exception manifests by code.</summary>
public interface ISeasonalExceptionManifestStore
{
    SeasonalExceptionManifest? Find(string code);
    void Save(SeasonalExceptionManifest item);
    bool Remove(string code);
    List<SeasonalExceptionManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalExceptionManifestStore(Dictionary<string, SeasonalExceptionManifest> items, List<string> order) : ISeasonalExceptionManifestStore
{
    public Dictionary<string, SeasonalExceptionManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalExceptionManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalExceptionManifest item)
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

    public List<SeasonalExceptionManifest> List()
    {
        var outValue = new List<SeasonalExceptionManifest>();
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
public sealed class CachedSeasonalExceptionManifestStore(MemorySeasonalExceptionManifestStore inner, SeasonalExceptionManifestCache cache, int hits, int misses) : ISeasonalExceptionManifestStore
{
    public MemorySeasonalExceptionManifestStore Inner { get; set; } = inner;
    public SeasonalExceptionManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalExceptionManifest? Find(string code)
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

    public void Save(SeasonalExceptionManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalExceptionManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal exception manifests. Empty fields match all.</summary>
public sealed record SeasonalExceptionManifestQuery(string Label, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalExceptionManifest item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalExceptionManifest> Apply(List<SeasonalExceptionManifest> items)
    {
        var matched = Collections.Filter(items, (SeasonalExceptionManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal exception manifest store.</summary>
public static class SeasonalExceptionManifestStore
{
    public static ISeasonalExceptionManifestStore NewMemorySeasonalExceptionManifestStore()
    {
        return new MemorySeasonalExceptionManifestStore(
            new Dictionary<string, SeasonalExceptionManifest>(),
            new List<string>());
    }

    public static ISeasonalExceptionManifestStore NewCachedSeasonalExceptionManifestStore(MemorySeasonalExceptionManifestStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalExceptionManifestStore(
            inner,
            SeasonalExceptionManifestCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalExceptionManifest> FindSeasonalExceptionManifests(ISeasonalExceptionManifestStore store, SeasonalExceptionManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
