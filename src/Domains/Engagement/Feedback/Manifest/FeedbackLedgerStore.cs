namespace Acme.Workforce.Engagement.Feedback.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback ledgers by code.</summary>
public interface IFeedbackLedgerStore
{
    FeedbackLedger? Find(string code);
    void Save(FeedbackLedger item);
    bool Remove(string code);
    List<FeedbackLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackLedgerStore(Dictionary<string, FeedbackLedger> items, List<string> order) : IFeedbackLedgerStore
{
    public Dictionary<string, FeedbackLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackLedger item)
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

    public List<FeedbackLedger> List()
    {
        var outValue = new List<FeedbackLedger>();
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
public sealed class CachedFeedbackLedgerStore(MemoryFeedbackLedgerStore inner, FeedbackLedgerCache cache, int hits, int misses) : IFeedbackLedgerStore
{
    public MemoryFeedbackLedgerStore Inner { get; set; } = inner;
    public FeedbackLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackLedger? Find(string code)
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

    public void Save(FeedbackLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback ledgers. Empty fields match all.</summary>
public sealed record FeedbackLedgerQuery(string Label, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackLedger item)
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

    public List<FeedbackLedger> Apply(List<FeedbackLedger> items)
    {
        var matched = Collections.Filter(items, (FeedbackLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback ledger store.</summary>
public static class FeedbackLedgerStore
{
    public static IFeedbackLedgerStore NewMemoryFeedbackLedgerStore()
    {
        return new MemoryFeedbackLedgerStore(new Dictionary<string, FeedbackLedger>(), new List<string>());
    }

    public static IFeedbackLedgerStore NewCachedFeedbackLedgerStore(MemoryFeedbackLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackLedgerStore(inner, FeedbackLedgerCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<FeedbackLedger> FindFeedbackLedgers(IFeedbackLedgerStore store, FeedbackLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
