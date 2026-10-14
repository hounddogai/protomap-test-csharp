namespace Acme.Workforce.Orgdesign.Scenario.WeightedScenarioTemplate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary scenario agreements by code.</summary>
public interface IPrimaryScenarioAgreementStore
{
    PrimaryScenarioAgreement? Find(string code);
    void Save(PrimaryScenarioAgreement item);
    bool Remove(string code);
    List<PrimaryScenarioAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryScenarioAgreementStore(Dictionary<string, PrimaryScenarioAgreement> items, List<string> order) : IPrimaryScenarioAgreementStore
{
    public Dictionary<string, PrimaryScenarioAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryScenarioAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryScenarioAgreement item)
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

    public List<PrimaryScenarioAgreement> List()
    {
        var outValue = new List<PrimaryScenarioAgreement>();
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
public sealed class CachedPrimaryScenarioAgreementStore(MemoryPrimaryScenarioAgreementStore inner, PrimaryScenarioAgreementCache cache, int hits, int misses) : IPrimaryScenarioAgreementStore
{
    public MemoryPrimaryScenarioAgreementStore Inner { get; set; } = inner;
    public PrimaryScenarioAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryScenarioAgreement? Find(string code)
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

    public void Save(PrimaryScenarioAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryScenarioAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary scenario agreements. Empty fields match all.</summary>
public sealed record PrimaryScenarioAgreementQuery(string Team, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryScenarioAgreement item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryScenarioAgreement> Apply(List<PrimaryScenarioAgreement> items)
    {
        var matched = Collections.Filter(items, (PrimaryScenarioAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary scenario agreement store.</summary>
public static class PrimaryScenarioAgreementStore
{
    public static IPrimaryScenarioAgreementStore NewMemoryPrimaryScenarioAgreementStore()
    {
        return new MemoryPrimaryScenarioAgreementStore(
            new Dictionary<string, PrimaryScenarioAgreement>(),
            new List<string>());
    }

    public static IPrimaryScenarioAgreementStore NewCachedPrimaryScenarioAgreementStore(MemoryPrimaryScenarioAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryScenarioAgreementStore(
            inner,
            PrimaryScenarioAgreementCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryScenarioAgreement> FindPrimaryScenarioAgreements(IPrimaryScenarioAgreementStore store, PrimaryScenarioAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
