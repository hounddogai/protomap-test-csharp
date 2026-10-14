namespace Acme.Workforce.Engagement.PulseSurvey.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending pulse survey audits by code.</summary>
public interface IPendingPulseSurveyAuditStore
{
    PendingPulseSurveyAudit? Find(string code);
    void Save(PendingPulseSurveyAudit item);
    bool Remove(string code);
    List<PendingPulseSurveyAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingPulseSurveyAuditStore(Dictionary<string, PendingPulseSurveyAudit> items, List<string> order) : IPendingPulseSurveyAuditStore
{
    public Dictionary<string, PendingPulseSurveyAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingPulseSurveyAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingPulseSurveyAudit item)
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

    public List<PendingPulseSurveyAudit> List()
    {
        var outValue = new List<PendingPulseSurveyAudit>();
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
public sealed class CachedPendingPulseSurveyAuditStore(MemoryPendingPulseSurveyAuditStore inner, PendingPulseSurveyAuditCache cache, int hits, int misses) : IPendingPulseSurveyAuditStore
{
    public MemoryPendingPulseSurveyAuditStore Inner { get; set; } = inner;
    public PendingPulseSurveyAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingPulseSurveyAudit? Find(string code)
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

    public void Save(PendingPulseSurveyAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingPulseSurveyAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending pulse survey audits. Empty fields match all.</summary>
public sealed record PendingPulseSurveyAuditQuery(string Locale, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(PendingPulseSurveyAudit item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<PendingPulseSurveyAudit> Apply(List<PendingPulseSurveyAudit> items)
    {
        var matched = Collections.Filter(items, (PendingPulseSurveyAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending pulse survey audit store.</summary>
public static class PendingPulseSurveyAuditStore
{
    public static IPendingPulseSurveyAuditStore NewMemoryPendingPulseSurveyAuditStore()
    {
        return new MemoryPendingPulseSurveyAuditStore(
            new Dictionary<string, PendingPulseSurveyAudit>(),
            new List<string>());
    }

    public static IPendingPulseSurveyAuditStore NewCachedPendingPulseSurveyAuditStore(MemoryPendingPulseSurveyAuditStore inner, int ttl, IClock clock)
    {
        return new CachedPendingPulseSurveyAuditStore(
            inner,
            PendingPulseSurveyAuditCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PendingPulseSurveyAudit> FindPendingPulseSurveyAudits(IPendingPulseSurveyAuditStore store, PendingPulseSurveyAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
