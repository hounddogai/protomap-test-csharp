namespace Acme.Workforce.Assets.Assignment.AssignmentClaim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered assignment claims by code.</summary>
public interface ITieredAssignmentClaimStore
{
    TieredAssignmentClaim? Find(string code);
    void Save(TieredAssignmentClaim item);
    bool Remove(string code);
    List<TieredAssignmentClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredAssignmentClaimStore(Dictionary<string, TieredAssignmentClaim> items, List<string> order) : ITieredAssignmentClaimStore
{
    public Dictionary<string, TieredAssignmentClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredAssignmentClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredAssignmentClaim item)
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

    public List<TieredAssignmentClaim> List()
    {
        var outValue = new List<TieredAssignmentClaim>();
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
public sealed class CachedTieredAssignmentClaimStore(MemoryTieredAssignmentClaimStore inner, TieredAssignmentClaimCache cache, int hits, int misses) : ITieredAssignmentClaimStore
{
    public MemoryTieredAssignmentClaimStore Inner { get; set; } = inner;
    public TieredAssignmentClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredAssignmentClaim? Find(string code)
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

    public void Save(TieredAssignmentClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredAssignmentClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered assignment claims. Empty fields match all.</summary>
public sealed record TieredAssignmentClaimQuery(string Name, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(TieredAssignmentClaim item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredAssignmentClaim> Apply(List<TieredAssignmentClaim> items)
    {
        var matched = Collections.Filter(items, (TieredAssignmentClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered assignment claim store.</summary>
public static class TieredAssignmentClaimStore
{
    public static ITieredAssignmentClaimStore NewMemoryTieredAssignmentClaimStore()
    {
        return new MemoryTieredAssignmentClaimStore(
            new Dictionary<string, TieredAssignmentClaim>(),
            new List<string>());
    }

    public static ITieredAssignmentClaimStore NewCachedTieredAssignmentClaimStore(MemoryTieredAssignmentClaimStore inner, int ttl, IClock clock)
    {
        return new CachedTieredAssignmentClaimStore(inner, TieredAssignmentClaimCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TieredAssignmentClaim> FindTieredAssignmentClaims(ITieredAssignmentClaimStore store, TieredAssignmentClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
