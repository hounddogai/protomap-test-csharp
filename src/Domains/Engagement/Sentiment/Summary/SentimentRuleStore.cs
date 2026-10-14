namespace Acme.Workforce.Engagement.Sentiment.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment rules by code.</summary>
public interface ISentimentRuleStore
{
    SentimentRule? Find(string code);
    void Save(SentimentRule item);
    bool Remove(string code);
    List<SentimentRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentRuleStore(Dictionary<string, SentimentRule> items, List<string> order) : ISentimentRuleStore
{
    public Dictionary<string, SentimentRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentRule item)
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

    public List<SentimentRule> List()
    {
        var outValue = new List<SentimentRule>();
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
public sealed class CachedSentimentRuleStore(MemorySentimentRuleStore inner, SentimentRuleCache cache, int hits, int misses) : ISentimentRuleStore
{
    public MemorySentimentRuleStore Inner { get; set; } = inner;
    public SentimentRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentRule? Find(string code)
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

    public void Save(SentimentRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment rules. Empty fields match all.</summary>
public sealed record SentimentRuleQuery(string Channel, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentRule item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SentimentRule> Apply(List<SentimentRule> items)
    {
        var matched = Collections.Filter(items, (SentimentRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment rule store.</summary>
public static class SentimentRuleStore
{
    public static ISentimentRuleStore NewMemorySentimentRuleStore()
    {
        return new MemorySentimentRuleStore(new Dictionary<string, SentimentRule>(), new List<string>());
    }

    public static ISentimentRuleStore NewCachedSentimentRuleStore(MemorySentimentRuleStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentRuleStore(inner, SentimentRuleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SentimentRule> FindSentimentRules(ISentimentRuleStore store, SentimentRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
