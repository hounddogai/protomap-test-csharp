namespace Acme.Workforce.Engagement.PulseSurvey.SurveyAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped pulse survey audits by code.</summary>
public interface ICappedPulseSurveyAuditStore
{
    CappedPulseSurveyAudit? Find(string code);
    void Save(CappedPulseSurveyAudit item);
    bool Remove(string code);
    List<CappedPulseSurveyAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedPulseSurveyAuditStore(Dictionary<string, CappedPulseSurveyAudit> items, List<string> order) : ICappedPulseSurveyAuditStore
{
    public Dictionary<string, CappedPulseSurveyAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedPulseSurveyAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedPulseSurveyAudit item)
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

    public List<CappedPulseSurveyAudit> List()
    {
        var outValue = new List<CappedPulseSurveyAudit>();
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
public sealed class CachedCappedPulseSurveyAuditStore(MemoryCappedPulseSurveyAuditStore inner, CappedPulseSurveyAuditCache cache, int hits, int misses) : ICappedPulseSurveyAuditStore
{
    public MemoryCappedPulseSurveyAuditStore Inner { get; set; } = inner;
    public CappedPulseSurveyAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedPulseSurveyAudit? Find(string code)
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

    public void Save(CappedPulseSurveyAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedPulseSurveyAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped pulse survey audits. Empty fields match all.</summary>
public sealed record CappedPulseSurveyAuditQuery(string Locale, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(CappedPulseSurveyAudit item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedPulseSurveyAudit> Apply(List<CappedPulseSurveyAudit> items)
    {
        var matched = Collections.Filter(items, (CappedPulseSurveyAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped pulse survey audit store.</summary>
public static class CappedPulseSurveyAuditStore
{
    public static ICappedPulseSurveyAuditStore NewMemoryCappedPulseSurveyAuditStore()
    {
        return new MemoryCappedPulseSurveyAuditStore(
            new Dictionary<string, CappedPulseSurveyAudit>(),
            new List<string>());
    }

    public static ICappedPulseSurveyAuditStore NewCachedCappedPulseSurveyAuditStore(MemoryCappedPulseSurveyAuditStore inner, int ttl, IClock clock)
    {
        return new CachedCappedPulseSurveyAuditStore(
            inner,
            CappedPulseSurveyAuditCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<CappedPulseSurveyAudit> FindCappedPulseSurveyAudits(ICappedPulseSurveyAuditStore store, CappedPulseSurveyAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
