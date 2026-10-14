namespace Acme.Workforce.Compliance.Certification.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual certification claims by code.</summary>
public interface IManualCertificationClaimStore
{
    ManualCertificationClaim? Find(string code);
    void Save(ManualCertificationClaim item);
    bool Remove(string code);
    List<ManualCertificationClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualCertificationClaimStore(Dictionary<string, ManualCertificationClaim> items, List<string> order) : IManualCertificationClaimStore
{
    public Dictionary<string, ManualCertificationClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualCertificationClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualCertificationClaim item)
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

    public List<ManualCertificationClaim> List()
    {
        var outValue = new List<ManualCertificationClaim>();
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
public sealed class CachedManualCertificationClaimStore(MemoryManualCertificationClaimStore inner, ManualCertificationClaimCache cache, int hits, int misses) : IManualCertificationClaimStore
{
    public MemoryManualCertificationClaimStore Inner { get; set; } = inner;
    public ManualCertificationClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualCertificationClaim? Find(string code)
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

    public void Save(ManualCertificationClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualCertificationClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual certification claims. Empty fields match all.</summary>
public sealed record ManualCertificationClaimQuery(string Name, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ManualCertificationClaim item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualCertificationClaim> Apply(List<ManualCertificationClaim> items)
    {
        var matched = Collections.Filter(items, (ManualCertificationClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual certification claim store.</summary>
public static class ManualCertificationClaimStore
{
    public static IManualCertificationClaimStore NewMemoryManualCertificationClaimStore()
    {
        return new MemoryManualCertificationClaimStore(
            new Dictionary<string, ManualCertificationClaim>(),
            new List<string>());
    }

    public static IManualCertificationClaimStore NewCachedManualCertificationClaimStore(MemoryManualCertificationClaimStore inner, int ttl, IClock clock)
    {
        return new CachedManualCertificationClaimStore(
            inner,
            ManualCertificationClaimCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ManualCertificationClaim> FindManualCertificationClaims(IManualCertificationClaimStore store, ManualCertificationClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
