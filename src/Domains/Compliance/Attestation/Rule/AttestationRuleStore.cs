namespace Acme.Workforce.Compliance.Attestation.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation rules by code.</summary>
public interface IAttestationRuleStore
{
    AttestationRule? Find(string code);
    void Save(AttestationRule item);
    bool Remove(string code);
    List<AttestationRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationRuleStore(Dictionary<string, AttestationRule> items, List<string> order) : IAttestationRuleStore
{
    public Dictionary<string, AttestationRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationRule item)
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

    public List<AttestationRule> List()
    {
        var outValue = new List<AttestationRule>();
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
public sealed class CachedAttestationRuleStore(MemoryAttestationRuleStore inner, AttestationRuleCache cache, int hits, int misses) : IAttestationRuleStore
{
    public MemoryAttestationRuleStore Inner { get; set; } = inner;
    public AttestationRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationRule? Find(string code)
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

    public void Save(AttestationRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation rules. Empty fields match all.</summary>
public sealed record AttestationRuleQuery(string Title, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationRule item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationRule> Apply(List<AttestationRule> items)
    {
        var matched = Collections.Filter(items, (AttestationRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation rule store.</summary>
public static class AttestationRuleStore
{
    public static IAttestationRuleStore NewMemoryAttestationRuleStore()
    {
        return new MemoryAttestationRuleStore(new Dictionary<string, AttestationRule>(), new List<string>());
    }

    public static IAttestationRuleStore NewCachedAttestationRuleStore(MemoryAttestationRuleStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationRuleStore(inner, AttestationRuleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AttestationRule> FindAttestationRules(IAttestationRuleStore store, AttestationRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
