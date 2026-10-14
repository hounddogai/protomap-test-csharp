namespace Acme.Workforce.Engagement.PulseSurvey.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey digests by code.</summary>
public interface IPulseSurveyDigestStore
{
    PulseSurveyDigest? Find(string code);
    void Save(PulseSurveyDigest item);
    bool Remove(string code);
    List<PulseSurveyDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyDigestStore(Dictionary<string, PulseSurveyDigest> items, List<string> order) : IPulseSurveyDigestStore
{
    public Dictionary<string, PulseSurveyDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyDigest item)
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

    public List<PulseSurveyDigest> List()
    {
        var outValue = new List<PulseSurveyDigest>();
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
public sealed class CachedPulseSurveyDigestStore(MemoryPulseSurveyDigestStore inner, PulseSurveyDigestCache cache, int hits, int misses) : IPulseSurveyDigestStore
{
    public MemoryPulseSurveyDigestStore Inner { get; set; } = inner;
    public PulseSurveyDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyDigest? Find(string code)
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

    public void Save(PulseSurveyDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey digests. Empty fields match all.</summary>
public sealed record PulseSurveyDigestQuery(string Channel, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyDigest item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PulseSurveyDigest> Apply(List<PulseSurveyDigest> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey digest store.</summary>
public static class PulseSurveyDigestStore
{
    public static IPulseSurveyDigestStore NewMemoryPulseSurveyDigestStore()
    {
        return new MemoryPulseSurveyDigestStore(new Dictionary<string, PulseSurveyDigest>(), new List<string>());
    }

    public static IPulseSurveyDigestStore NewCachedPulseSurveyDigestStore(MemoryPulseSurveyDigestStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyDigestStore(inner, PulseSurveyDigestCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PulseSurveyDigest> FindPulseSurveyDigests(IPulseSurveyDigestStore store, PulseSurveyDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
