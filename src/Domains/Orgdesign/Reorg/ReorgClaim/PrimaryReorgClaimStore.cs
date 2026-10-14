namespace Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary reorg claims by code.</summary>
public interface IPrimaryReorgClaimStore
{
    PrimaryReorgClaim? Find(string code);
    void Save(PrimaryReorgClaim item);
    bool Remove(string code);
    List<PrimaryReorgClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryReorgClaimStore(Dictionary<string, PrimaryReorgClaim> items, List<string> order) : IPrimaryReorgClaimStore
{
    public Dictionary<string, PrimaryReorgClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryReorgClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryReorgClaim item)
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

    public List<PrimaryReorgClaim> List()
    {
        var outValue = new List<PrimaryReorgClaim>();
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
public sealed class CachedPrimaryReorgClaimStore(MemoryPrimaryReorgClaimStore inner, PrimaryReorgClaimCache cache, int hits, int misses) : IPrimaryReorgClaimStore
{
    public MemoryPrimaryReorgClaimStore Inner { get; set; } = inner;
    public PrimaryReorgClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryReorgClaim? Find(string code)
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

    public void Save(PrimaryReorgClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryReorgClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary reorg claims. Empty fields match all.</summary>
public sealed record PrimaryReorgClaimQuery(string Source, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryReorgClaim item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryReorgClaim> Apply(List<PrimaryReorgClaim> items)
    {
        var matched = Collections.Filter(items, (PrimaryReorgClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary reorg claim store.</summary>
public static class PrimaryReorgClaimStore
{
    public static IPrimaryReorgClaimStore NewMemoryPrimaryReorgClaimStore()
    {
        return new MemoryPrimaryReorgClaimStore(new Dictionary<string, PrimaryReorgClaim>(), new List<string>());
    }

    public static IPrimaryReorgClaimStore NewCachedPrimaryReorgClaimStore(MemoryPrimaryReorgClaimStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryReorgClaimStore(inner, PrimaryReorgClaimCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PrimaryReorgClaim> FindPrimaryReorgClaims(IPrimaryReorgClaimStore store, PrimaryReorgClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
