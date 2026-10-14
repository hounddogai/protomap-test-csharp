namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary attestation scores by code.</summary>
public interface IPrimaryAttestationScoreStore
{
    PrimaryAttestationScore? Find(string code);
    void Save(PrimaryAttestationScore item);
    bool Remove(string code);
    List<PrimaryAttestationScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryAttestationScoreStore(Dictionary<string, PrimaryAttestationScore> items, List<string> order) : IPrimaryAttestationScoreStore
{
    public Dictionary<string, PrimaryAttestationScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryAttestationScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryAttestationScore item)
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

    public List<PrimaryAttestationScore> List()
    {
        var outValue = new List<PrimaryAttestationScore>();
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
public sealed class CachedPrimaryAttestationScoreStore(MemoryPrimaryAttestationScoreStore inner, PrimaryAttestationScoreCache cache, int hits, int misses) : IPrimaryAttestationScoreStore
{
    public MemoryPrimaryAttestationScoreStore Inner { get; set; } = inner;
    public PrimaryAttestationScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryAttestationScore? Find(string code)
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

    public void Save(PrimaryAttestationScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryAttestationScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary attestation scores. Empty fields match all.</summary>
public sealed record PrimaryAttestationScoreQuery(string Team, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryAttestationScore item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryAttestationScore> Apply(List<PrimaryAttestationScore> items)
    {
        var matched = Collections.Filter(items, (PrimaryAttestationScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary attestation score store.</summary>
public static class PrimaryAttestationScoreStore
{
    public static IPrimaryAttestationScoreStore NewMemoryPrimaryAttestationScoreStore()
    {
        return new MemoryPrimaryAttestationScoreStore(
            new Dictionary<string, PrimaryAttestationScore>(),
            new List<string>());
    }

    public static IPrimaryAttestationScoreStore NewCachedPrimaryAttestationScoreStore(MemoryPrimaryAttestationScoreStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryAttestationScoreStore(
            inner,
            PrimaryAttestationScoreCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryAttestationScore> FindPrimaryAttestationScores(IPrimaryAttestationScoreStore store, PrimaryAttestationScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
