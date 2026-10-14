namespace Acme.Workforce.Engagement.Recognition.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional recognition thresholds by code.</summary>
public interface IRegionalRecognitionThresholdStore
{
    RegionalRecognitionThreshold? Find(string code);
    void Save(RegionalRecognitionThreshold item);
    bool Remove(string code);
    List<RegionalRecognitionThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalRecognitionThresholdStore(Dictionary<string, RegionalRecognitionThreshold> items, List<string> order) : IRegionalRecognitionThresholdStore
{
    public Dictionary<string, RegionalRecognitionThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalRecognitionThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalRecognitionThreshold item)
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

    public List<RegionalRecognitionThreshold> List()
    {
        var outValue = new List<RegionalRecognitionThreshold>();
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
public sealed class CachedRegionalRecognitionThresholdStore(MemoryRegionalRecognitionThresholdStore inner, RegionalRecognitionThresholdCache cache, int hits, int misses) : IRegionalRecognitionThresholdStore
{
    public MemoryRegionalRecognitionThresholdStore Inner { get; set; } = inner;
    public RegionalRecognitionThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalRecognitionThreshold? Find(string code)
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

    public void Save(RegionalRecognitionThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalRecognitionThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional recognition thresholds. Empty fields match all.</summary>
public sealed record RegionalRecognitionThresholdQuery(string Country, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalRecognitionThreshold item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<RegionalRecognitionThreshold> Apply(List<RegionalRecognitionThreshold> items)
    {
        var matched = Collections.Filter(items, (RegionalRecognitionThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional recognition threshold store.</summary>
public static class RegionalRecognitionThresholdStore
{
    public static IRegionalRecognitionThresholdStore NewMemoryRegionalRecognitionThresholdStore()
    {
        return new MemoryRegionalRecognitionThresholdStore(
            new Dictionary<string, RegionalRecognitionThreshold>(),
            new List<string>());
    }

    public static IRegionalRecognitionThresholdStore NewCachedRegionalRecognitionThresholdStore(MemoryRegionalRecognitionThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalRecognitionThresholdStore(
            inner,
            RegionalRecognitionThresholdCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RegionalRecognitionThreshold> FindRegionalRecognitionThresholds(IRegionalRecognitionThresholdStore store, RegionalRecognitionThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
