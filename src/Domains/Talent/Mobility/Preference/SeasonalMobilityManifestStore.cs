namespace Acme.Workforce.Talent.Mobility.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal mobility manifests by code.</summary>
public interface ISeasonalMobilityManifestStore
{
    SeasonalMobilityManifest? Find(string code);
    void Save(SeasonalMobilityManifest item);
    bool Remove(string code);
    List<SeasonalMobilityManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalMobilityManifestStore(Dictionary<string, SeasonalMobilityManifest> items, List<string> order) : ISeasonalMobilityManifestStore
{
    public Dictionary<string, SeasonalMobilityManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalMobilityManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalMobilityManifest item)
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

    public List<SeasonalMobilityManifest> List()
    {
        var outValue = new List<SeasonalMobilityManifest>();
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
public sealed class CachedSeasonalMobilityManifestStore(MemorySeasonalMobilityManifestStore inner, SeasonalMobilityManifestCache cache, int hits, int misses) : ISeasonalMobilityManifestStore
{
    public MemorySeasonalMobilityManifestStore Inner { get; set; } = inner;
    public SeasonalMobilityManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalMobilityManifest? Find(string code)
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

    public void Save(SeasonalMobilityManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalMobilityManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal mobility manifests. Empty fields match all.</summary>
public sealed record SeasonalMobilityManifestQuery(string Unit, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalMobilityManifest item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalMobilityManifest> Apply(List<SeasonalMobilityManifest> items)
    {
        var matched = Collections.Filter(items, (SeasonalMobilityManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal mobility manifest store.</summary>
public static class SeasonalMobilityManifestStore
{
    public static ISeasonalMobilityManifestStore NewMemorySeasonalMobilityManifestStore()
    {
        return new MemorySeasonalMobilityManifestStore(
            new Dictionary<string, SeasonalMobilityManifest>(),
            new List<string>());
    }

    public static ISeasonalMobilityManifestStore NewCachedSeasonalMobilityManifestStore(MemorySeasonalMobilityManifestStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalMobilityManifestStore(
            inner,
            SeasonalMobilityManifestCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalMobilityManifest> FindSeasonalMobilityManifests(ISeasonalMobilityManifestStore store, SeasonalMobilityManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
