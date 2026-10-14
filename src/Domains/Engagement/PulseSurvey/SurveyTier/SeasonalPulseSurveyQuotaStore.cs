namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal pulse survey quotas by code.</summary>
public interface ISeasonalPulseSurveyQuotaStore
{
    SeasonalPulseSurveyQuota? Find(string code);
    void Save(SeasonalPulseSurveyQuota item);
    bool Remove(string code);
    List<SeasonalPulseSurveyQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalPulseSurveyQuotaStore(Dictionary<string, SeasonalPulseSurveyQuota> items, List<string> order) : ISeasonalPulseSurveyQuotaStore
{
    public Dictionary<string, SeasonalPulseSurveyQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalPulseSurveyQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalPulseSurveyQuota item)
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

    public List<SeasonalPulseSurveyQuota> List()
    {
        var outValue = new List<SeasonalPulseSurveyQuota>();
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
public sealed class CachedSeasonalPulseSurveyQuotaStore(MemorySeasonalPulseSurveyQuotaStore inner, SeasonalPulseSurveyQuotaCache cache, int hits, int misses) : ISeasonalPulseSurveyQuotaStore
{
    public MemorySeasonalPulseSurveyQuotaStore Inner { get; set; } = inner;
    public SeasonalPulseSurveyQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalPulseSurveyQuota? Find(string code)
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

    public void Save(SeasonalPulseSurveyQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalPulseSurveyQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal pulse survey quotas. Empty fields match all.</summary>
public sealed record SeasonalPulseSurveyQuotaQuery(string Channel, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalPulseSurveyQuota item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalPulseSurveyQuota> Apply(List<SeasonalPulseSurveyQuota> items)
    {
        var matched = Collections.Filter(items, (SeasonalPulseSurveyQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal pulse survey quota store.</summary>
public static class SeasonalPulseSurveyQuotaStore
{
    public static ISeasonalPulseSurveyQuotaStore NewMemorySeasonalPulseSurveyQuotaStore()
    {
        return new MemorySeasonalPulseSurveyQuotaStore(
            new Dictionary<string, SeasonalPulseSurveyQuota>(),
            new List<string>());
    }

    public static ISeasonalPulseSurveyQuotaStore NewCachedSeasonalPulseSurveyQuotaStore(MemorySeasonalPulseSurveyQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalPulseSurveyQuotaStore(
            inner,
            SeasonalPulseSurveyQuotaCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalPulseSurveyQuota> FindSeasonalPulseSurveyQuotas(ISeasonalPulseSurveyQuotaStore store, SeasonalPulseSurveyQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
