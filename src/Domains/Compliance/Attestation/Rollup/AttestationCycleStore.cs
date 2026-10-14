namespace Acme.Workforce.Compliance.Attestation.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation cycles by code.</summary>
public interface IAttestationCycleStore
{
    AttestationCycle? Find(string code);
    void Save(AttestationCycle item);
    bool Remove(string code);
    List<AttestationCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationCycleStore(Dictionary<string, AttestationCycle> items, List<string> order) : IAttestationCycleStore
{
    public Dictionary<string, AttestationCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationCycle item)
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

    public List<AttestationCycle> List()
    {
        var outValue = new List<AttestationCycle>();
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
public sealed class CachedAttestationCycleStore(MemoryAttestationCycleStore inner, AttestationCycleCache cache, int hits, int misses) : IAttestationCycleStore
{
    public MemoryAttestationCycleStore Inner { get; set; } = inner;
    public AttestationCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationCycle? Find(string code)
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

    public void Save(AttestationCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation cycles. Empty fields match all.</summary>
public sealed record AttestationCycleQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationCycle item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationCycle> Apply(List<AttestationCycle> items)
    {
        var matched = Collections.Filter(items, (AttestationCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation cycle store.</summary>
public static class AttestationCycleStore
{
    public static IAttestationCycleStore NewMemoryAttestationCycleStore()
    {
        return new MemoryAttestationCycleStore(new Dictionary<string, AttestationCycle>(), new List<string>());
    }

    public static IAttestationCycleStore NewCachedAttestationCycleStore(MemoryAttestationCycleStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationCycleStore(inner, AttestationCycleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AttestationCycle> FindAttestationCycles(IAttestationCycleStore store, AttestationCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
