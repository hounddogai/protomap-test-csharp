namespace Acme.Workforce.Assets.Assignment.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment claims by code.</summary>
public interface IAssignmentClaimStore
{
    AssignmentClaim? Find(string code);
    void Save(AssignmentClaim item);
    bool Remove(string code);
    List<AssignmentClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentClaimStore(Dictionary<string, AssignmentClaim> items, List<string> order) : IAssignmentClaimStore
{
    public Dictionary<string, AssignmentClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentClaim item)
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

    public List<AssignmentClaim> List()
    {
        var outValue = new List<AssignmentClaim>();
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
public sealed class CachedAssignmentClaimStore(MemoryAssignmentClaimStore inner, AssignmentClaimCache cache, int hits, int misses) : IAssignmentClaimStore
{
    public MemoryAssignmentClaimStore Inner { get; set; } = inner;
    public AssignmentClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentClaim? Find(string code)
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

    public void Save(AssignmentClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment claims. Empty fields match all.</summary>
public sealed record AssignmentClaimQuery(string Name, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentClaim item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentClaim> Apply(List<AssignmentClaim> items)
    {
        var matched = Collections.Filter(items, (AssignmentClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment claim store.</summary>
public static class AssignmentClaimStore
{
    public static IAssignmentClaimStore NewMemoryAssignmentClaimStore()
    {
        return new MemoryAssignmentClaimStore(new Dictionary<string, AssignmentClaim>(), new List<string>());
    }

    public static IAssignmentClaimStore NewCachedAssignmentClaimStore(MemoryAssignmentClaimStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentClaimStore(inner, AssignmentClaimCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AssignmentClaim> FindAssignmentClaims(IAssignmentClaimStore store, AssignmentClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
