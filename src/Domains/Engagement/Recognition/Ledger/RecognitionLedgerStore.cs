namespace Acme.Workforce.Engagement.Recognition.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition ledgers by code.</summary>
public interface IRecognitionLedgerStore
{
    RecognitionLedger? Find(string code);
    void Save(RecognitionLedger item);
    bool Remove(string code);
    List<RecognitionLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionLedgerStore(Dictionary<string, RecognitionLedger> items, List<string> order) : IRecognitionLedgerStore
{
    public Dictionary<string, RecognitionLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionLedger item)
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

    public List<RecognitionLedger> List()
    {
        var outValue = new List<RecognitionLedger>();
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
public sealed class CachedRecognitionLedgerStore(MemoryRecognitionLedgerStore inner, RecognitionLedgerCache cache, int hits, int misses) : IRecognitionLedgerStore
{
    public MemoryRecognitionLedgerStore Inner { get; set; } = inner;
    public RecognitionLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionLedger? Find(string code)
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

    public void Save(RecognitionLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition ledgers. Empty fields match all.</summary>
public sealed record RecognitionLedgerQuery(string Unit, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionLedger item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RecognitionLedger> Apply(List<RecognitionLedger> items)
    {
        var matched = Collections.Filter(items, (RecognitionLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition ledger store.</summary>
public static class RecognitionLedgerStore
{
    public static IRecognitionLedgerStore NewMemoryRecognitionLedgerStore()
    {
        return new MemoryRecognitionLedgerStore(new Dictionary<string, RecognitionLedger>(), new List<string>());
    }

    public static IRecognitionLedgerStore NewCachedRecognitionLedgerStore(MemoryRecognitionLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionLedgerStore(inner, RecognitionLedgerCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RecognitionLedger> FindRecognitionLedgers(IRecognitionLedgerStore store, RecognitionLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
