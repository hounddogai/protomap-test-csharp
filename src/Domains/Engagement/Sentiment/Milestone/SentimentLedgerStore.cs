namespace Acme.Workforce.Engagement.Sentiment.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment ledgers by code.</summary>
public interface ISentimentLedgerStore
{
    SentimentLedger? Find(string code);
    void Save(SentimentLedger item);
    bool Remove(string code);
    List<SentimentLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentLedgerStore(Dictionary<string, SentimentLedger> items, List<string> order) : ISentimentLedgerStore
{
    public Dictionary<string, SentimentLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentLedger item)
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

    public List<SentimentLedger> List()
    {
        var outValue = new List<SentimentLedger>();
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
public sealed class CachedSentimentLedgerStore(MemorySentimentLedgerStore inner, SentimentLedgerCache cache, int hits, int misses) : ISentimentLedgerStore
{
    public MemorySentimentLedgerStore Inner { get; set; } = inner;
    public SentimentLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentLedger? Find(string code)
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

    public void Save(SentimentLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment ledgers. Empty fields match all.</summary>
public sealed record SentimentLedgerQuery(string Country, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentLedger item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SentimentLedger> Apply(List<SentimentLedger> items)
    {
        var matched = Collections.Filter(items, (SentimentLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment ledger store.</summary>
public static class SentimentLedgerStore
{
    public static ISentimentLedgerStore NewMemorySentimentLedgerStore()
    {
        return new MemorySentimentLedgerStore(new Dictionary<string, SentimentLedger>(), new List<string>());
    }

    public static ISentimentLedgerStore NewCachedSentimentLedgerStore(MemorySentimentLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentLedgerStore(inner, SentimentLedgerCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<SentimentLedger> FindSentimentLedgers(ISentimentLedgerStore store, SentimentLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
