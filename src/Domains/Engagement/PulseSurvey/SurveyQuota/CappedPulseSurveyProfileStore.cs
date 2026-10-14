namespace Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped pulse survey profiles by code.</summary>
public interface ICappedPulseSurveyProfileStore
{
    CappedPulseSurveyProfile? Find(string code);
    void Save(CappedPulseSurveyProfile item);
    bool Remove(string code);
    List<CappedPulseSurveyProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedPulseSurveyProfileStore(Dictionary<string, CappedPulseSurveyProfile> items, List<string> order) : ICappedPulseSurveyProfileStore
{
    public Dictionary<string, CappedPulseSurveyProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedPulseSurveyProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedPulseSurveyProfile item)
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

    public List<CappedPulseSurveyProfile> List()
    {
        var outValue = new List<CappedPulseSurveyProfile>();
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
public sealed class CachedCappedPulseSurveyProfileStore(MemoryCappedPulseSurveyProfileStore inner, CappedPulseSurveyProfileCache cache, int hits, int misses) : ICappedPulseSurveyProfileStore
{
    public MemoryCappedPulseSurveyProfileStore Inner { get; set; } = inner;
    public CappedPulseSurveyProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedPulseSurveyProfile? Find(string code)
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

    public void Save(CappedPulseSurveyProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedPulseSurveyProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped pulse survey profiles. Empty fields match all.</summary>
public sealed record CappedPulseSurveyProfileQuery(string Title, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(CappedPulseSurveyProfile item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedPulseSurveyProfile> Apply(List<CappedPulseSurveyProfile> items)
    {
        var matched = Collections.Filter(items, (CappedPulseSurveyProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped pulse survey profile store.</summary>
public static class CappedPulseSurveyProfileStore
{
    public static ICappedPulseSurveyProfileStore NewMemoryCappedPulseSurveyProfileStore()
    {
        return new MemoryCappedPulseSurveyProfileStore(
            new Dictionary<string, CappedPulseSurveyProfile>(),
            new List<string>());
    }

    public static ICappedPulseSurveyProfileStore NewCachedCappedPulseSurveyProfileStore(MemoryCappedPulseSurveyProfileStore inner, int ttl, IClock clock)
    {
        return new CachedCappedPulseSurveyProfileStore(
            inner,
            CappedPulseSurveyProfileCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<CappedPulseSurveyProfile> FindCappedPulseSurveyProfiles(ICappedPulseSurveyProfileStore store, CappedPulseSurveyProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
