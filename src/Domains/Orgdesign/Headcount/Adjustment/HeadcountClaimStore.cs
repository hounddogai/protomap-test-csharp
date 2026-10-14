namespace Acme.Workforce.Orgdesign.Headcount.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount claims by code.</summary>
public interface IHeadcountClaimStore
{
    HeadcountClaim? Find(string code);
    void Save(HeadcountClaim item);
    bool Remove(string code);
    List<HeadcountClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountClaimStore(Dictionary<string, HeadcountClaim> items, List<string> order) : IHeadcountClaimStore
{
    public Dictionary<string, HeadcountClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountClaim item)
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

    public List<HeadcountClaim> List()
    {
        var outValue = new List<HeadcountClaim>();
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
public sealed class CachedHeadcountClaimStore(MemoryHeadcountClaimStore inner, HeadcountClaimCache cache, int hits, int misses) : IHeadcountClaimStore
{
    public MemoryHeadcountClaimStore Inner { get; set; } = inner;
    public HeadcountClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountClaim? Find(string code)
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

    public void Save(HeadcountClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount claims. Empty fields match all.</summary>
public sealed record HeadcountClaimQuery(string Channel, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountClaim item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<HeadcountClaim> Apply(List<HeadcountClaim> items)
    {
        var matched = Collections.Filter(items, (HeadcountClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount claim store.</summary>
public static class HeadcountClaimStore
{
    public static IHeadcountClaimStore NewMemoryHeadcountClaimStore()
    {
        return new MemoryHeadcountClaimStore(new Dictionary<string, HeadcountClaim>(), new List<string>());
    }

    public static IHeadcountClaimStore NewCachedHeadcountClaimStore(MemoryHeadcountClaimStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountClaimStore(inner, HeadcountClaimCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<HeadcountClaim> FindHeadcountClaims(IHeadcountClaimStore store, HeadcountClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
