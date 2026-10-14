namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered headcount claims by code.</summary>
public interface ITieredHeadcountClaimStore
{
    TieredHeadcountClaim? Find(string code);
    void Save(TieredHeadcountClaim item);
    bool Remove(string code);
    List<TieredHeadcountClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredHeadcountClaimStore(Dictionary<string, TieredHeadcountClaim> items, List<string> order) : ITieredHeadcountClaimStore
{
    public Dictionary<string, TieredHeadcountClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredHeadcountClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredHeadcountClaim item)
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

    public List<TieredHeadcountClaim> List()
    {
        var outValue = new List<TieredHeadcountClaim>();
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
public sealed class CachedTieredHeadcountClaimStore(MemoryTieredHeadcountClaimStore inner, TieredHeadcountClaimCache cache, int hits, int misses) : ITieredHeadcountClaimStore
{
    public MemoryTieredHeadcountClaimStore Inner { get; set; } = inner;
    public TieredHeadcountClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredHeadcountClaim? Find(string code)
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

    public void Save(TieredHeadcountClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredHeadcountClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered headcount claims. Empty fields match all.</summary>
public sealed record TieredHeadcountClaimQuery(string Unit, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(TieredHeadcountClaim item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<TieredHeadcountClaim> Apply(List<TieredHeadcountClaim> items)
    {
        var matched = Collections.Filter(items, (TieredHeadcountClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered headcount claim store.</summary>
public static class TieredHeadcountClaimStore
{
    public static ITieredHeadcountClaimStore NewMemoryTieredHeadcountClaimStore()
    {
        return new MemoryTieredHeadcountClaimStore(
            new Dictionary<string, TieredHeadcountClaim>(),
            new List<string>());
    }

    public static ITieredHeadcountClaimStore NewCachedTieredHeadcountClaimStore(MemoryTieredHeadcountClaimStore inner, int ttl, IClock clock)
    {
        return new CachedTieredHeadcountClaimStore(inner, TieredHeadcountClaimCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TieredHeadcountClaim> FindTieredHeadcountClaims(ITieredHeadcountClaimStore store, TieredHeadcountClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
