namespace Acme.Workforce.Orgdesign.Scenario.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending scenario agreements by code.</summary>
public interface IPendingScenarioAgreementStore
{
    PendingScenarioAgreement? Find(string code);
    void Save(PendingScenarioAgreement item);
    bool Remove(string code);
    List<PendingScenarioAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingScenarioAgreementStore(Dictionary<string, PendingScenarioAgreement> items, List<string> order) : IPendingScenarioAgreementStore
{
    public Dictionary<string, PendingScenarioAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingScenarioAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingScenarioAgreement item)
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

    public List<PendingScenarioAgreement> List()
    {
        var outValue = new List<PendingScenarioAgreement>();
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
public sealed class CachedPendingScenarioAgreementStore(MemoryPendingScenarioAgreementStore inner, PendingScenarioAgreementCache cache, int hits, int misses) : IPendingScenarioAgreementStore
{
    public MemoryPendingScenarioAgreementStore Inner { get; set; } = inner;
    public PendingScenarioAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingScenarioAgreement? Find(string code)
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

    public void Save(PendingScenarioAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingScenarioAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending scenario agreements. Empty fields match all.</summary>
public sealed record PendingScenarioAgreementQuery(string Locale, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(PendingScenarioAgreement item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingScenarioAgreement> Apply(List<PendingScenarioAgreement> items)
    {
        var matched = Collections.Filter(items, (PendingScenarioAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending scenario agreement store.</summary>
public static class PendingScenarioAgreementStore
{
    public static IPendingScenarioAgreementStore NewMemoryPendingScenarioAgreementStore()
    {
        return new MemoryPendingScenarioAgreementStore(
            new Dictionary<string, PendingScenarioAgreement>(),
            new List<string>());
    }

    public static IPendingScenarioAgreementStore NewCachedPendingScenarioAgreementStore(MemoryPendingScenarioAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedPendingScenarioAgreementStore(
            inner,
            PendingScenarioAgreementCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PendingScenarioAgreement> FindPendingScenarioAgreements(IPendingScenarioAgreementStore store, PendingScenarioAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
