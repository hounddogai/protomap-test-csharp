namespace Acme.Workforce.Engagement.Sentiment.SentimentVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment agreements by code.</summary>
public interface ISentimentAgreementStore
{
    SentimentAgreement? Find(string code);
    void Save(SentimentAgreement item);
    bool Remove(string code);
    List<SentimentAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentAgreementStore(Dictionary<string, SentimentAgreement> items, List<string> order) : ISentimentAgreementStore
{
    public Dictionary<string, SentimentAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentAgreement item)
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

    public List<SentimentAgreement> List()
    {
        var outValue = new List<SentimentAgreement>();
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
public sealed class CachedSentimentAgreementStore(MemorySentimentAgreementStore inner, SentimentAgreementCache cache, int hits, int misses) : ISentimentAgreementStore
{
    public MemorySentimentAgreementStore Inner { get; set; } = inner;
    public SentimentAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentAgreement? Find(string code)
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

    public void Save(SentimentAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment agreements. Empty fields match all.</summary>
public sealed record SentimentAgreementQuery(string Name, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentAgreement item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SentimentAgreement> Apply(List<SentimentAgreement> items)
    {
        var matched = Collections.Filter(items, (SentimentAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment agreement store.</summary>
public static class SentimentAgreementStore
{
    public static ISentimentAgreementStore NewMemorySentimentAgreementStore()
    {
        return new MemorySentimentAgreementStore(new Dictionary<string, SentimentAgreement>(), new List<string>());
    }

    public static ISentimentAgreementStore NewCachedSentimentAgreementStore(MemorySentimentAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentAgreementStore(inner, SentimentAgreementCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<SentimentAgreement> FindSentimentAgreements(ISentimentAgreementStore store, SentimentAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
