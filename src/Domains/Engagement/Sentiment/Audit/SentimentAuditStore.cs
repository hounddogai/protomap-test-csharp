namespace Acme.Workforce.Engagement.Sentiment.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment audits by code.</summary>
public interface ISentimentAuditStore
{
    SentimentAudit? Find(string code);
    void Save(SentimentAudit item);
    bool Remove(string code);
    List<SentimentAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentAuditStore(Dictionary<string, SentimentAudit> items, List<string> order) : ISentimentAuditStore
{
    public Dictionary<string, SentimentAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentAudit item)
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

    public List<SentimentAudit> List()
    {
        var outValue = new List<SentimentAudit>();
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
public sealed class CachedSentimentAuditStore(MemorySentimentAuditStore inner, SentimentAuditCache cache, int hits, int misses) : ISentimentAuditStore
{
    public MemorySentimentAuditStore Inner { get; set; } = inner;
    public SentimentAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentAudit? Find(string code)
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

    public void Save(SentimentAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment audits. Empty fields match all.</summary>
public sealed record SentimentAuditQuery(string Department, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentAudit item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SentimentAudit> Apply(List<SentimentAudit> items)
    {
        var matched = Collections.Filter(items, (SentimentAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment audit store.</summary>
public static class SentimentAuditStore
{
    public static ISentimentAuditStore NewMemorySentimentAuditStore()
    {
        return new MemorySentimentAuditStore(new Dictionary<string, SentimentAudit>(), new List<string>());
    }

    public static ISentimentAuditStore NewCachedSentimentAuditStore(MemorySentimentAuditStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentAuditStore(inner, SentimentAuditCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SentimentAudit> FindSentimentAudits(ISentimentAuditStore store, SentimentAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
