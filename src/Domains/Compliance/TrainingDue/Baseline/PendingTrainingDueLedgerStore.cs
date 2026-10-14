namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending training due ledgers by code.</summary>
public interface IPendingTrainingDueLedgerStore
{
    PendingTrainingDueLedger? Find(string code);
    void Save(PendingTrainingDueLedger item);
    bool Remove(string code);
    List<PendingTrainingDueLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingTrainingDueLedgerStore(Dictionary<string, PendingTrainingDueLedger> items, List<string> order) : IPendingTrainingDueLedgerStore
{
    public Dictionary<string, PendingTrainingDueLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingTrainingDueLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingTrainingDueLedger item)
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

    public List<PendingTrainingDueLedger> List()
    {
        var outValue = new List<PendingTrainingDueLedger>();
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
public sealed class CachedPendingTrainingDueLedgerStore(MemoryPendingTrainingDueLedgerStore inner, PendingTrainingDueLedgerCache cache, int hits, int misses) : IPendingTrainingDueLedgerStore
{
    public MemoryPendingTrainingDueLedgerStore Inner { get; set; } = inner;
    public PendingTrainingDueLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingTrainingDueLedger? Find(string code)
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

    public void Save(PendingTrainingDueLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingTrainingDueLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending training due ledgers. Empty fields match all.</summary>
public sealed record PendingTrainingDueLedgerQuery(string Segment, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(PendingTrainingDueLedger item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingTrainingDueLedger> Apply(List<PendingTrainingDueLedger> items)
    {
        var matched = Collections.Filter(items, (PendingTrainingDueLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending training due ledger store.</summary>
public static class PendingTrainingDueLedgerStore
{
    public static IPendingTrainingDueLedgerStore NewMemoryPendingTrainingDueLedgerStore()
    {
        return new MemoryPendingTrainingDueLedgerStore(
            new Dictionary<string, PendingTrainingDueLedger>(),
            new List<string>());
    }

    public static IPendingTrainingDueLedgerStore NewCachedPendingTrainingDueLedgerStore(MemoryPendingTrainingDueLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedPendingTrainingDueLedgerStore(
            inner,
            PendingTrainingDueLedgerCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PendingTrainingDueLedger> FindPendingTrainingDueLedgers(IPendingTrainingDueLedgerStore store, PendingTrainingDueLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
