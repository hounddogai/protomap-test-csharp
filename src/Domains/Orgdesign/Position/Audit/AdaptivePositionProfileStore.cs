namespace Acme.Workforce.Orgdesign.Position.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive position profiles by code.</summary>
public interface IAdaptivePositionProfileStore
{
    AdaptivePositionProfile? Find(string code);
    void Save(AdaptivePositionProfile item);
    bool Remove(string code);
    List<AdaptivePositionProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptivePositionProfileStore(Dictionary<string, AdaptivePositionProfile> items, List<string> order) : IAdaptivePositionProfileStore
{
    public Dictionary<string, AdaptivePositionProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptivePositionProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptivePositionProfile item)
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

    public List<AdaptivePositionProfile> List()
    {
        var outValue = new List<AdaptivePositionProfile>();
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
public sealed class CachedAdaptivePositionProfileStore(MemoryAdaptivePositionProfileStore inner, AdaptivePositionProfileCache cache, int hits, int misses) : IAdaptivePositionProfileStore
{
    public MemoryAdaptivePositionProfileStore Inner { get; set; } = inner;
    public AdaptivePositionProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptivePositionProfile? Find(string code)
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

    public void Save(AdaptivePositionProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptivePositionProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive position profiles. Empty fields match all.</summary>
public sealed record AdaptivePositionProfileQuery(string Unit, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptivePositionProfile item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptivePositionProfile> Apply(List<AdaptivePositionProfile> items)
    {
        var matched = Collections.Filter(items, (AdaptivePositionProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive position profile store.</summary>
public static class AdaptivePositionProfileStore
{
    public static IAdaptivePositionProfileStore NewMemoryAdaptivePositionProfileStore()
    {
        return new MemoryAdaptivePositionProfileStore(
            new Dictionary<string, AdaptivePositionProfile>(),
            new List<string>());
    }

    public static IAdaptivePositionProfileStore NewCachedAdaptivePositionProfileStore(MemoryAdaptivePositionProfileStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptivePositionProfileStore(
            inner,
            AdaptivePositionProfileCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<AdaptivePositionProfile> FindAdaptivePositionProfiles(IAdaptivePositionProfileStore store, AdaptivePositionProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
