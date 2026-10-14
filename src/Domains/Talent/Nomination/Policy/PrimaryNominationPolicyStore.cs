namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary nomination policies by code.</summary>
public interface IPrimaryNominationPolicyStore
{
    PrimaryNominationPolicy? Find(string code);
    void Save(PrimaryNominationPolicy item);
    bool Remove(string code);
    List<PrimaryNominationPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryNominationPolicyStore(Dictionary<string, PrimaryNominationPolicy> items, List<string> order) : IPrimaryNominationPolicyStore
{
    public Dictionary<string, PrimaryNominationPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryNominationPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryNominationPolicy item)
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

    public List<PrimaryNominationPolicy> List()
    {
        var outValue = new List<PrimaryNominationPolicy>();
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
public sealed class CachedPrimaryNominationPolicyStore(MemoryPrimaryNominationPolicyStore inner, PrimaryNominationPolicyCache cache, int hits, int misses) : IPrimaryNominationPolicyStore
{
    public MemoryPrimaryNominationPolicyStore Inner { get; set; } = inner;
    public PrimaryNominationPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryNominationPolicy? Find(string code)
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

    public void Save(PrimaryNominationPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryNominationPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary nomination policies. Empty fields match all.</summary>
public sealed record PrimaryNominationPolicyQuery(string Title, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryNominationPolicy item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryNominationPolicy> Apply(List<PrimaryNominationPolicy> items)
    {
        var matched = Collections.Filter(items, (PrimaryNominationPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary nomination policy store.</summary>
public static class PrimaryNominationPolicyStore
{
    public static IPrimaryNominationPolicyStore NewMemoryPrimaryNominationPolicyStore()
    {
        return new MemoryPrimaryNominationPolicyStore(
            new Dictionary<string, PrimaryNominationPolicy>(),
            new List<string>());
    }

    public static IPrimaryNominationPolicyStore NewCachedPrimaryNominationPolicyStore(MemoryPrimaryNominationPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryNominationPolicyStore(
            inner,
            PrimaryNominationPolicyCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryNominationPolicy> FindPrimaryNominationPolicies(IPrimaryNominationPolicyStore store, PrimaryNominationPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
