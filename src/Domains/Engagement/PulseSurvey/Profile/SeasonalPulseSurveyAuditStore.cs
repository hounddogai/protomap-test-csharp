namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal pulse survey audits by code.</summary>
public interface ISeasonalPulseSurveyAuditStore
{
    SeasonalPulseSurveyAudit? Find(string code);
    void Save(SeasonalPulseSurveyAudit item);
    bool Remove(string code);
    List<SeasonalPulseSurveyAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalPulseSurveyAuditStore(Dictionary<string, SeasonalPulseSurveyAudit> items, List<string> order) : ISeasonalPulseSurveyAuditStore
{
    public Dictionary<string, SeasonalPulseSurveyAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalPulseSurveyAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalPulseSurveyAudit item)
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

    public List<SeasonalPulseSurveyAudit> List()
    {
        var outValue = new List<SeasonalPulseSurveyAudit>();
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
public sealed class CachedSeasonalPulseSurveyAuditStore(MemorySeasonalPulseSurveyAuditStore inner, SeasonalPulseSurveyAuditCache cache, int hits, int misses) : ISeasonalPulseSurveyAuditStore
{
    public MemorySeasonalPulseSurveyAuditStore Inner { get; set; } = inner;
    public SeasonalPulseSurveyAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalPulseSurveyAudit? Find(string code)
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

    public void Save(SeasonalPulseSurveyAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalPulseSurveyAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal pulse survey audits. Empty fields match all.</summary>
public sealed record SeasonalPulseSurveyAuditQuery(string Label, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalPulseSurveyAudit item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalPulseSurveyAudit> Apply(List<SeasonalPulseSurveyAudit> items)
    {
        var matched = Collections.Filter(items, (SeasonalPulseSurveyAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal pulse survey audit store.</summary>
public static class SeasonalPulseSurveyAuditStore
{
    public static ISeasonalPulseSurveyAuditStore NewMemorySeasonalPulseSurveyAuditStore()
    {
        return new MemorySeasonalPulseSurveyAuditStore(
            new Dictionary<string, SeasonalPulseSurveyAudit>(),
            new List<string>());
    }

    public static ISeasonalPulseSurveyAuditStore NewCachedSeasonalPulseSurveyAuditStore(MemorySeasonalPulseSurveyAuditStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalPulseSurveyAuditStore(
            inner,
            SeasonalPulseSurveyAuditCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalPulseSurveyAudit> FindSeasonalPulseSurveyAudits(ISeasonalPulseSurveyAuditStore store, SeasonalPulseSurveyAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
