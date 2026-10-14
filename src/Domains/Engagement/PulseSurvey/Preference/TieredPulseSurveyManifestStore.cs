namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered pulse survey manifests by code.</summary>
public interface ITieredPulseSurveyManifestStore
{
    TieredPulseSurveyManifest? Find(string code);
    void Save(TieredPulseSurveyManifest item);
    bool Remove(string code);
    List<TieredPulseSurveyManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredPulseSurveyManifestStore(Dictionary<string, TieredPulseSurveyManifest> items, List<string> order) : ITieredPulseSurveyManifestStore
{
    public Dictionary<string, TieredPulseSurveyManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredPulseSurveyManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredPulseSurveyManifest item)
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

    public List<TieredPulseSurveyManifest> List()
    {
        var outValue = new List<TieredPulseSurveyManifest>();
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
public sealed class CachedTieredPulseSurveyManifestStore(MemoryTieredPulseSurveyManifestStore inner, TieredPulseSurveyManifestCache cache, int hits, int misses) : ITieredPulseSurveyManifestStore
{
    public MemoryTieredPulseSurveyManifestStore Inner { get; set; } = inner;
    public TieredPulseSurveyManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredPulseSurveyManifest? Find(string code)
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

    public void Save(TieredPulseSurveyManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredPulseSurveyManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered pulse survey manifests. Empty fields match all.</summary>
public sealed record TieredPulseSurveyManifestQuery(string Title, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(TieredPulseSurveyManifest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<TieredPulseSurveyManifest> Apply(List<TieredPulseSurveyManifest> items)
    {
        var matched = Collections.Filter(items, (TieredPulseSurveyManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered pulse survey manifest store.</summary>
public static class TieredPulseSurveyManifestStore
{
    public static ITieredPulseSurveyManifestStore NewMemoryTieredPulseSurveyManifestStore()
    {
        return new MemoryTieredPulseSurveyManifestStore(
            new Dictionary<string, TieredPulseSurveyManifest>(),
            new List<string>());
    }

    public static ITieredPulseSurveyManifestStore NewCachedTieredPulseSurveyManifestStore(MemoryTieredPulseSurveyManifestStore inner, int ttl, IClock clock)
    {
        return new CachedTieredPulseSurveyManifestStore(
            inner,
            TieredPulseSurveyManifestCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<TieredPulseSurveyManifest> FindTieredPulseSurveyManifests(ITieredPulseSurveyManifestStore store, TieredPulseSurveyManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
