namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination policies by code.</summary>
public interface INominationPolicyStore
{
    NominationPolicy? Find(string code);
    void Save(NominationPolicy item);
    bool Remove(string code);
    List<NominationPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationPolicyStore(Dictionary<string, NominationPolicy> items, List<string> order) : INominationPolicyStore
{
    public Dictionary<string, NominationPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationPolicy item)
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

    public List<NominationPolicy> List()
    {
        var outValue = new List<NominationPolicy>();
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
public sealed class CachedNominationPolicyStore(MemoryNominationPolicyStore inner, NominationPolicyCache cache, int hits, int misses) : INominationPolicyStore
{
    public MemoryNominationPolicyStore Inner { get; set; } = inner;
    public NominationPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationPolicy? Find(string code)
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

    public void Save(NominationPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination policies. Empty fields match all.</summary>
public sealed record NominationPolicyQuery(string Label, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(NominationPolicy item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<NominationPolicy> Apply(List<NominationPolicy> items)
    {
        var matched = Collections.Filter(items, (NominationPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination policy store.</summary>
public static class NominationPolicyStore
{
    public static INominationPolicyStore NewMemoryNominationPolicyStore()
    {
        return new MemoryNominationPolicyStore(new Dictionary<string, NominationPolicy>(), new List<string>());
    }

    public static INominationPolicyStore NewCachedNominationPolicyStore(MemoryNominationPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedNominationPolicyStore(inner, NominationPolicyCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<NominationPolicy> FindNominationPolicies(INominationPolicyStore store, NominationPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
