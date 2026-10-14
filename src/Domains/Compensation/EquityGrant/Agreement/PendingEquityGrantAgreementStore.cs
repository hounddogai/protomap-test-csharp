namespace Acme.Workforce.Compensation.EquityGrant.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending equity grant agreements by code.</summary>
public interface IPendingEquityGrantAgreementStore
{
    PendingEquityGrantAgreement? Find(string code);
    void Save(PendingEquityGrantAgreement item);
    bool Remove(string code);
    List<PendingEquityGrantAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingEquityGrantAgreementStore(Dictionary<string, PendingEquityGrantAgreement> items, List<string> order) : IPendingEquityGrantAgreementStore
{
    public Dictionary<string, PendingEquityGrantAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingEquityGrantAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingEquityGrantAgreement item)
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

    public List<PendingEquityGrantAgreement> List()
    {
        var outValue = new List<PendingEquityGrantAgreement>();
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
public sealed class CachedPendingEquityGrantAgreementStore(MemoryPendingEquityGrantAgreementStore inner, PendingEquityGrantAgreementCache cache, int hits, int misses) : IPendingEquityGrantAgreementStore
{
    public MemoryPendingEquityGrantAgreementStore Inner { get; set; } = inner;
    public PendingEquityGrantAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingEquityGrantAgreement? Find(string code)
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

    public void Save(PendingEquityGrantAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingEquityGrantAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending equity grant agreements. Empty fields match all.</summary>
public sealed record PendingEquityGrantAgreementQuery(string Locale, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(PendingEquityGrantAgreement item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingEquityGrantAgreement> Apply(List<PendingEquityGrantAgreement> items)
    {
        var matched = Collections.Filter(items, (PendingEquityGrantAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending equity grant agreement store.</summary>
public static class PendingEquityGrantAgreementStore
{
    public static IPendingEquityGrantAgreementStore NewMemoryPendingEquityGrantAgreementStore()
    {
        return new MemoryPendingEquityGrantAgreementStore(
            new Dictionary<string, PendingEquityGrantAgreement>(),
            new List<string>());
    }

    public static IPendingEquityGrantAgreementStore NewCachedPendingEquityGrantAgreementStore(MemoryPendingEquityGrantAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedPendingEquityGrantAgreementStore(
            inner,
            PendingEquityGrantAgreementCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PendingEquityGrantAgreement> FindPendingEquityGrantAgreements(IPendingEquityGrantAgreementStore store, PendingEquityGrantAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
