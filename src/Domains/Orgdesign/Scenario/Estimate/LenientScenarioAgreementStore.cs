namespace Acme.Workforce.Orgdesign.Scenario.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient scenario agreements by code.</summary>
public interface ILenientScenarioAgreementStore
{
    LenientScenarioAgreement? Find(string code);
    void Save(LenientScenarioAgreement item);
    bool Remove(string code);
    List<LenientScenarioAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientScenarioAgreementStore(Dictionary<string, LenientScenarioAgreement> items, List<string> order) : ILenientScenarioAgreementStore
{
    public Dictionary<string, LenientScenarioAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientScenarioAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientScenarioAgreement item)
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

    public List<LenientScenarioAgreement> List()
    {
        var outValue = new List<LenientScenarioAgreement>();
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
public sealed class CachedLenientScenarioAgreementStore(MemoryLenientScenarioAgreementStore inner, LenientScenarioAgreementCache cache, int hits, int misses) : ILenientScenarioAgreementStore
{
    public MemoryLenientScenarioAgreementStore Inner { get; set; } = inner;
    public LenientScenarioAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientScenarioAgreement? Find(string code)
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

    public void Save(LenientScenarioAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientScenarioAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient scenario agreements. Empty fields match all.</summary>
public sealed record LenientScenarioAgreementQuery(string Label, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(LenientScenarioAgreement item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientScenarioAgreement> Apply(List<LenientScenarioAgreement> items)
    {
        var matched = Collections.Filter(items, (LenientScenarioAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient scenario agreement store.</summary>
public static class LenientScenarioAgreementStore
{
    public static ILenientScenarioAgreementStore NewMemoryLenientScenarioAgreementStore()
    {
        return new MemoryLenientScenarioAgreementStore(
            new Dictionary<string, LenientScenarioAgreement>(),
            new List<string>());
    }

    public static ILenientScenarioAgreementStore NewCachedLenientScenarioAgreementStore(MemoryLenientScenarioAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedLenientScenarioAgreementStore(
            inner,
            LenientScenarioAgreementCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LenientScenarioAgreement> FindLenientScenarioAgreements(ILenientScenarioAgreementStore store, LenientScenarioAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
