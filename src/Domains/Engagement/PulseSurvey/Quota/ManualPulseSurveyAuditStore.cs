namespace Acme.Workforce.Engagement.PulseSurvey.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual pulse survey audits by code.</summary>
public interface IManualPulseSurveyAuditStore
{
    ManualPulseSurveyAudit? Find(string code);
    void Save(ManualPulseSurveyAudit item);
    bool Remove(string code);
    List<ManualPulseSurveyAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualPulseSurveyAuditStore(Dictionary<string, ManualPulseSurveyAudit> items, List<string> order) : IManualPulseSurveyAuditStore
{
    public Dictionary<string, ManualPulseSurveyAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualPulseSurveyAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualPulseSurveyAudit item)
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

    public List<ManualPulseSurveyAudit> List()
    {
        var outValue = new List<ManualPulseSurveyAudit>();
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
public sealed class CachedManualPulseSurveyAuditStore(MemoryManualPulseSurveyAuditStore inner, ManualPulseSurveyAuditCache cache, int hits, int misses) : IManualPulseSurveyAuditStore
{
    public MemoryManualPulseSurveyAuditStore Inner { get; set; } = inner;
    public ManualPulseSurveyAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualPulseSurveyAudit? Find(string code)
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

    public void Save(ManualPulseSurveyAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualPulseSurveyAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual pulse survey audits. Empty fields match all.</summary>
public sealed record ManualPulseSurveyAuditQuery(string Label, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(ManualPulseSurveyAudit item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualPulseSurveyAudit> Apply(List<ManualPulseSurveyAudit> items)
    {
        var matched = Collections.Filter(items, (ManualPulseSurveyAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual pulse survey audit store.</summary>
public static class ManualPulseSurveyAuditStore
{
    public static IManualPulseSurveyAuditStore NewMemoryManualPulseSurveyAuditStore()
    {
        return new MemoryManualPulseSurveyAuditStore(
            new Dictionary<string, ManualPulseSurveyAudit>(),
            new List<string>());
    }

    public static IManualPulseSurveyAuditStore NewCachedManualPulseSurveyAuditStore(MemoryManualPulseSurveyAuditStore inner, int ttl, IClock clock)
    {
        return new CachedManualPulseSurveyAuditStore(
            inner,
            ManualPulseSurveyAuditCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ManualPulseSurveyAudit> FindManualPulseSurveyAudits(IManualPulseSurveyAuditStore store, ManualPulseSurveyAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
