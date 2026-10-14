namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient pulse survey manifests by code.</summary>
public interface ILenientPulseSurveyManifestStore
{
    LenientPulseSurveyManifest? Find(string code);
    void Save(LenientPulseSurveyManifest item);
    bool Remove(string code);
    List<LenientPulseSurveyManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientPulseSurveyManifestStore(Dictionary<string, LenientPulseSurveyManifest> items, List<string> order) : ILenientPulseSurveyManifestStore
{
    public Dictionary<string, LenientPulseSurveyManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientPulseSurveyManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientPulseSurveyManifest item)
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

    public List<LenientPulseSurveyManifest> List()
    {
        var outValue = new List<LenientPulseSurveyManifest>();
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
public sealed class CachedLenientPulseSurveyManifestStore(MemoryLenientPulseSurveyManifestStore inner, LenientPulseSurveyManifestCache cache, int hits, int misses) : ILenientPulseSurveyManifestStore
{
    public MemoryLenientPulseSurveyManifestStore Inner { get; set; } = inner;
    public LenientPulseSurveyManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientPulseSurveyManifest? Find(string code)
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

    public void Save(LenientPulseSurveyManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientPulseSurveyManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient pulse survey manifests. Empty fields match all.</summary>
public sealed record LenientPulseSurveyManifestQuery(string Label, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(LenientPulseSurveyManifest item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<LenientPulseSurveyManifest> Apply(List<LenientPulseSurveyManifest> items)
    {
        var matched = Collections.Filter(items, (LenientPulseSurveyManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient pulse survey manifest store.</summary>
public static class LenientPulseSurveyManifestStore
{
    public static ILenientPulseSurveyManifestStore NewMemoryLenientPulseSurveyManifestStore()
    {
        return new MemoryLenientPulseSurveyManifestStore(
            new Dictionary<string, LenientPulseSurveyManifest>(),
            new List<string>());
    }

    public static ILenientPulseSurveyManifestStore NewCachedLenientPulseSurveyManifestStore(MemoryLenientPulseSurveyManifestStore inner, int ttl, IClock clock)
    {
        return new CachedLenientPulseSurveyManifestStore(
            inner,
            LenientPulseSurveyManifestCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LenientPulseSurveyManifest> FindLenientPulseSurveyManifests(ILenientPulseSurveyManifestStore store, LenientPulseSurveyManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
