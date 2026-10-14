namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAgreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending adjustment agreements by code.</summary>
public interface IPendingAdjustmentAgreementStore
{
    PendingAdjustmentAgreement? Find(string code);
    void Save(PendingAdjustmentAgreement item);
    bool Remove(string code);
    List<PendingAdjustmentAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingAdjustmentAgreementStore(Dictionary<string, PendingAdjustmentAgreement> items, List<string> order) : IPendingAdjustmentAgreementStore
{
    public Dictionary<string, PendingAdjustmentAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingAdjustmentAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingAdjustmentAgreement item)
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

    public List<PendingAdjustmentAgreement> List()
    {
        var outValue = new List<PendingAdjustmentAgreement>();
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
public sealed class CachedPendingAdjustmentAgreementStore(MemoryPendingAdjustmentAgreementStore inner, PendingAdjustmentAgreementCache cache, int hits, int misses) : IPendingAdjustmentAgreementStore
{
    public MemoryPendingAdjustmentAgreementStore Inner { get; set; } = inner;
    public PendingAdjustmentAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingAdjustmentAgreement? Find(string code)
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

    public void Save(PendingAdjustmentAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingAdjustmentAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending adjustment agreements. Empty fields match all.</summary>
public sealed record PendingAdjustmentAgreementQuery(string Title, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(PendingAdjustmentAgreement item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingAdjustmentAgreement> Apply(List<PendingAdjustmentAgreement> items)
    {
        var matched = Collections.Filter(items, (PendingAdjustmentAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending adjustment agreement store.</summary>
public static class PendingAdjustmentAgreementStore
{
    public static IPendingAdjustmentAgreementStore NewMemoryPendingAdjustmentAgreementStore()
    {
        return new MemoryPendingAdjustmentAgreementStore(
            new Dictionary<string, PendingAdjustmentAgreement>(),
            new List<string>());
    }

    public static IPendingAdjustmentAgreementStore NewCachedPendingAdjustmentAgreementStore(MemoryPendingAdjustmentAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedPendingAdjustmentAgreementStore(
            inner,
            PendingAdjustmentAgreementCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PendingAdjustmentAgreement> FindPendingAdjustmentAgreements(IPendingAdjustmentAgreementStore store, PendingAdjustmentAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
