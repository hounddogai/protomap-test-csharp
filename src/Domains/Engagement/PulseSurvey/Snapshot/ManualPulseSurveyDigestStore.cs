namespace Acme.Workforce.Engagement.PulseSurvey.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual pulse survey digests by code.</summary>
public interface IManualPulseSurveyDigestStore
{
    ManualPulseSurveyDigest? Find(string code);
    void Save(ManualPulseSurveyDigest item);
    bool Remove(string code);
    List<ManualPulseSurveyDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualPulseSurveyDigestStore(Dictionary<string, ManualPulseSurveyDigest> items, List<string> order) : IManualPulseSurveyDigestStore
{
    public Dictionary<string, ManualPulseSurveyDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualPulseSurveyDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualPulseSurveyDigest item)
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

    public List<ManualPulseSurveyDigest> List()
    {
        var outValue = new List<ManualPulseSurveyDigest>();
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
public sealed class CachedManualPulseSurveyDigestStore(MemoryManualPulseSurveyDigestStore inner, ManualPulseSurveyDigestCache cache, int hits, int misses) : IManualPulseSurveyDigestStore
{
    public MemoryManualPulseSurveyDigestStore Inner { get; set; } = inner;
    public ManualPulseSurveyDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualPulseSurveyDigest? Find(string code)
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

    public void Save(ManualPulseSurveyDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualPulseSurveyDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual pulse survey digests. Empty fields match all.</summary>
public sealed record ManualPulseSurveyDigestQuery(string Name, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(ManualPulseSurveyDigest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualPulseSurveyDigest> Apply(List<ManualPulseSurveyDigest> items)
    {
        var matched = Collections.Filter(items, (ManualPulseSurveyDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual pulse survey digest store.</summary>
public static class ManualPulseSurveyDigestStore
{
    public static IManualPulseSurveyDigestStore NewMemoryManualPulseSurveyDigestStore()
    {
        return new MemoryManualPulseSurveyDigestStore(
            new Dictionary<string, ManualPulseSurveyDigest>(),
            new List<string>());
    }

    public static IManualPulseSurveyDigestStore NewCachedManualPulseSurveyDigestStore(MemoryManualPulseSurveyDigestStore inner, int ttl, IClock clock)
    {
        return new CachedManualPulseSurveyDigestStore(
            inner,
            ManualPulseSurveyDigestCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ManualPulseSurveyDigest> FindManualPulseSurveyDigests(IManualPulseSurveyDigestStore store, ManualPulseSurveyDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
