namespace Acme.Workforce.Compliance.Attestation.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation scores by code.</summary>
public interface IAttestationScoreStore
{
    AttestationScore? Find(string code);
    void Save(AttestationScore item);
    bool Remove(string code);
    List<AttestationScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationScoreStore(Dictionary<string, AttestationScore> items, List<string> order) : IAttestationScoreStore
{
    public Dictionary<string, AttestationScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationScore item)
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

    public List<AttestationScore> List()
    {
        var outValue = new List<AttestationScore>();
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
public sealed class CachedAttestationScoreStore(MemoryAttestationScoreStore inner, AttestationScoreCache cache, int hits, int misses) : IAttestationScoreStore
{
    public MemoryAttestationScoreStore Inner { get; set; } = inner;
    public AttestationScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationScore? Find(string code)
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

    public void Save(AttestationScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation scores. Empty fields match all.</summary>
public sealed record AttestationScoreQuery(string Region, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationScore item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationScore> Apply(List<AttestationScore> items)
    {
        var matched = Collections.Filter(items, (AttestationScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation score store.</summary>
public static class AttestationScoreStore
{
    public static IAttestationScoreStore NewMemoryAttestationScoreStore()
    {
        return new MemoryAttestationScoreStore(new Dictionary<string, AttestationScore>(), new List<string>());
    }

    public static IAttestationScoreStore NewCachedAttestationScoreStore(MemoryAttestationScoreStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationScoreStore(inner, AttestationScoreCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AttestationScore> FindAttestationScores(IAttestationScoreStore store, AttestationScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
