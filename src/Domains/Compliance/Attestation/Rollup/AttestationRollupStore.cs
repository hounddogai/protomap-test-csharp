namespace Acme.Workforce.Compliance.Attestation.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation rollups by code.</summary>
public interface IAttestationRollupStore
{
    AttestationRollup? Find(string code);
    void Save(AttestationRollup item);
    bool Remove(string code);
    List<AttestationRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationRollupStore(Dictionary<string, AttestationRollup> items, List<string> order) : IAttestationRollupStore
{
    public Dictionary<string, AttestationRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationRollup item)
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

    public List<AttestationRollup> List()
    {
        var outValue = new List<AttestationRollup>();
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
public sealed class CachedAttestationRollupStore(MemoryAttestationRollupStore inner, AttestationRollupCache cache, int hits, int misses) : IAttestationRollupStore
{
    public MemoryAttestationRollupStore Inner { get; set; } = inner;
    public AttestationRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationRollup? Find(string code)
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

    public void Save(AttestationRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation rollups. Empty fields match all.</summary>
public sealed record AttestationRollupQuery(string Name, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationRollup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<AttestationRollup> Apply(List<AttestationRollup> items)
    {
        var matched = Collections.Filter(items, (AttestationRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation rollup store.</summary>
public static class AttestationRollupStore
{
    public static IAttestationRollupStore NewMemoryAttestationRollupStore()
    {
        return new MemoryAttestationRollupStore(new Dictionary<string, AttestationRollup>(), new List<string>());
    }

    public static IAttestationRollupStore NewCachedAttestationRollupStore(MemoryAttestationRollupStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationRollupStore(inner, AttestationRollupCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AttestationRollup> FindAttestationRollups(IAttestationRollupStore store, AttestationRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
