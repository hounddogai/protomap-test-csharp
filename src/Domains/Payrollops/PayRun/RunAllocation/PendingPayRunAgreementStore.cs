namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending pay run agreements by code.</summary>
public interface IPendingPayRunAgreementStore
{
    PendingPayRunAgreement? Find(string code);
    void Save(PendingPayRunAgreement item);
    bool Remove(string code);
    List<PendingPayRunAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingPayRunAgreementStore(Dictionary<string, PendingPayRunAgreement> items, List<string> order) : IPendingPayRunAgreementStore
{
    public Dictionary<string, PendingPayRunAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingPayRunAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingPayRunAgreement item)
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

    public List<PendingPayRunAgreement> List()
    {
        var outValue = new List<PendingPayRunAgreement>();
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
public sealed class CachedPendingPayRunAgreementStore(MemoryPendingPayRunAgreementStore inner, PendingPayRunAgreementCache cache, int hits, int misses) : IPendingPayRunAgreementStore
{
    public MemoryPendingPayRunAgreementStore Inner { get; set; } = inner;
    public PendingPayRunAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingPayRunAgreement? Find(string code)
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

    public void Save(PendingPayRunAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingPayRunAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending pay run agreements. Empty fields match all.</summary>
public sealed record PendingPayRunAgreementQuery(string Title, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(PendingPayRunAgreement item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingPayRunAgreement> Apply(List<PendingPayRunAgreement> items)
    {
        var matched = Collections.Filter(items, (PendingPayRunAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending pay run agreement store.</summary>
public static class PendingPayRunAgreementStore
{
    public static IPendingPayRunAgreementStore NewMemoryPendingPayRunAgreementStore()
    {
        return new MemoryPendingPayRunAgreementStore(
            new Dictionary<string, PendingPayRunAgreement>(),
            new List<string>());
    }

    public static IPendingPayRunAgreementStore NewCachedPendingPayRunAgreementStore(MemoryPendingPayRunAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedPendingPayRunAgreementStore(
            inner,
            PendingPayRunAgreementCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PendingPayRunAgreement> FindPendingPayRunAgreements(IPendingPayRunAgreementStore store, PendingPayRunAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
