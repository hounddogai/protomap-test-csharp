namespace Acme.Workforce.Engagement.PulseSurvey.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary pulse survey manifests by code.</summary>
public interface IPrimaryPulseSurveyManifestStore
{
    PrimaryPulseSurveyManifest? Find(string code);
    void Save(PrimaryPulseSurveyManifest item);
    bool Remove(string code);
    List<PrimaryPulseSurveyManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryPulseSurveyManifestStore(Dictionary<string, PrimaryPulseSurveyManifest> items, List<string> order) : IPrimaryPulseSurveyManifestStore
{
    public Dictionary<string, PrimaryPulseSurveyManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryPulseSurveyManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryPulseSurveyManifest item)
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

    public List<PrimaryPulseSurveyManifest> List()
    {
        var outValue = new List<PrimaryPulseSurveyManifest>();
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
public sealed class CachedPrimaryPulseSurveyManifestStore(MemoryPrimaryPulseSurveyManifestStore inner, PrimaryPulseSurveyManifestCache cache, int hits, int misses) : IPrimaryPulseSurveyManifestStore
{
    public MemoryPrimaryPulseSurveyManifestStore Inner { get; set; } = inner;
    public PrimaryPulseSurveyManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryPulseSurveyManifest? Find(string code)
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

    public void Save(PrimaryPulseSurveyManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryPulseSurveyManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary pulse survey manifests. Empty fields match all.</summary>
public sealed record PrimaryPulseSurveyManifestQuery(string Channel, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryPulseSurveyManifest item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryPulseSurveyManifest> Apply(List<PrimaryPulseSurveyManifest> items)
    {
        var matched = Collections.Filter(items, (PrimaryPulseSurveyManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary pulse survey manifest store.</summary>
public static class PrimaryPulseSurveyManifestStore
{
    public static IPrimaryPulseSurveyManifestStore NewMemoryPrimaryPulseSurveyManifestStore()
    {
        return new MemoryPrimaryPulseSurveyManifestStore(
            new Dictionary<string, PrimaryPulseSurveyManifest>(),
            new List<string>());
    }

    public static IPrimaryPulseSurveyManifestStore NewCachedPrimaryPulseSurveyManifestStore(MemoryPrimaryPulseSurveyManifestStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryPulseSurveyManifestStore(
            inner,
            PrimaryPulseSurveyManifestCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryPulseSurveyManifest> FindPrimaryPulseSurveyManifests(IPrimaryPulseSurveyManifestStore store, PrimaryPulseSurveyManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
