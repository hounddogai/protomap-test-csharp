namespace Acme.Workforce.Compliance.Attestation.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation plans by code.</summary>
public interface IAttestationPlanStore
{
    AttestationPlan? Find(string code);
    void Save(AttestationPlan item);
    bool Remove(string code);
    List<AttestationPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationPlanStore(Dictionary<string, AttestationPlan> items, List<string> order) : IAttestationPlanStore
{
    public Dictionary<string, AttestationPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationPlan item)
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

    public List<AttestationPlan> List()
    {
        var outValue = new List<AttestationPlan>();
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
public sealed class CachedAttestationPlanStore(MemoryAttestationPlanStore inner, AttestationPlanCache cache, int hits, int misses) : IAttestationPlanStore
{
    public MemoryAttestationPlanStore Inner { get; set; } = inner;
    public AttestationPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationPlan? Find(string code)
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

    public void Save(AttestationPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation plans. Empty fields match all.</summary>
public sealed record AttestationPlanQuery(string Team, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationPlan item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationPlan> Apply(List<AttestationPlan> items)
    {
        var matched = Collections.Filter(items, (AttestationPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation plan store.</summary>
public static class AttestationPlanStore
{
    public static IAttestationPlanStore NewMemoryAttestationPlanStore()
    {
        return new MemoryAttestationPlanStore(new Dictionary<string, AttestationPlan>(), new List<string>());
    }

    public static IAttestationPlanStore NewCachedAttestationPlanStore(MemoryAttestationPlanStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationPlanStore(inner, AttestationPlanCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AttestationPlan> FindAttestationPlans(IAttestationPlanStore store, AttestationPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
