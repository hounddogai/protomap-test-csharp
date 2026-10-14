namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled sentiment ledgers by code.</summary>
public interface IScheduledSentimentLedgerStore
{
    ScheduledSentimentLedger? Find(string code);
    void Save(ScheduledSentimentLedger item);
    bool Remove(string code);
    List<ScheduledSentimentLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledSentimentLedgerStore(Dictionary<string, ScheduledSentimentLedger> items, List<string> order) : IScheduledSentimentLedgerStore
{
    public Dictionary<string, ScheduledSentimentLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledSentimentLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledSentimentLedger item)
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

    public List<ScheduledSentimentLedger> List()
    {
        var outValue = new List<ScheduledSentimentLedger>();
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
public sealed class CachedScheduledSentimentLedgerStore(MemoryScheduledSentimentLedgerStore inner, ScheduledSentimentLedgerCache cache, int hits, int misses) : IScheduledSentimentLedgerStore
{
    public MemoryScheduledSentimentLedgerStore Inner { get; set; } = inner;
    public ScheduledSentimentLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledSentimentLedger? Find(string code)
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

    public void Save(ScheduledSentimentLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledSentimentLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled sentiment ledgers. Empty fields match all.</summary>
public sealed record ScheduledSentimentLedgerQuery(string Channel, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledSentimentLedger item)
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

    public List<ScheduledSentimentLedger> Apply(List<ScheduledSentimentLedger> items)
    {
        var matched = Collections.Filter(items, (ScheduledSentimentLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled sentiment ledger store.</summary>
public static class ScheduledSentimentLedgerStore
{
    public static IScheduledSentimentLedgerStore NewMemoryScheduledSentimentLedgerStore()
    {
        return new MemoryScheduledSentimentLedgerStore(
            new Dictionary<string, ScheduledSentimentLedger>(),
            new List<string>());
    }

    public static IScheduledSentimentLedgerStore NewCachedScheduledSentimentLedgerStore(MemoryScheduledSentimentLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledSentimentLedgerStore(
            inner,
            ScheduledSentimentLedgerCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledSentimentLedger> FindScheduledSentimentLedgers(IScheduledSentimentLedgerStore store, ScheduledSentimentLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
