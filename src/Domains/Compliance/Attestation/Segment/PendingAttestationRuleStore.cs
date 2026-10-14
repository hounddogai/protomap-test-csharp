namespace Acme.Workforce.Compliance.Attestation.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending attestation rules by code.</summary>
public interface IPendingAttestationRuleStore
{
    PendingAttestationRule? Find(string code);
    void Save(PendingAttestationRule item);
    bool Remove(string code);
    List<PendingAttestationRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingAttestationRuleStore(Dictionary<string, PendingAttestationRule> items, List<string> order) : IPendingAttestationRuleStore
{
    public Dictionary<string, PendingAttestationRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingAttestationRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingAttestationRule item)
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

    public List<PendingAttestationRule> List()
    {
        var outValue = new List<PendingAttestationRule>();
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
public sealed class CachedPendingAttestationRuleStore(MemoryPendingAttestationRuleStore inner, PendingAttestationRuleCache cache, int hits, int misses) : IPendingAttestationRuleStore
{
    public MemoryPendingAttestationRuleStore Inner { get; set; } = inner;
    public PendingAttestationRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingAttestationRule? Find(string code)
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

    public void Save(PendingAttestationRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingAttestationRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending attestation rules. Empty fields match all.</summary>
public sealed record PendingAttestationRuleQuery(string Title, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(PendingAttestationRule item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingAttestationRule> Apply(List<PendingAttestationRule> items)
    {
        var matched = Collections.Filter(items, (PendingAttestationRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending attestation rule store.</summary>
public static class PendingAttestationRuleStore
{
    public static IPendingAttestationRuleStore NewMemoryPendingAttestationRuleStore()
    {
        return new MemoryPendingAttestationRuleStore(
            new Dictionary<string, PendingAttestationRule>(),
            new List<string>());
    }

    public static IPendingAttestationRuleStore NewCachedPendingAttestationRuleStore(MemoryPendingAttestationRuleStore inner, int ttl, IClock clock)
    {
        return new CachedPendingAttestationRuleStore(
            inner,
            PendingAttestationRuleCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PendingAttestationRule> FindPendingAttestationRules(IPendingAttestationRuleStore store, PendingAttestationRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
