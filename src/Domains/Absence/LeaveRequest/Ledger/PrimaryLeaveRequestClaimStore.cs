namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary leave request claims by code.</summary>
public interface IPrimaryLeaveRequestClaimStore
{
    PrimaryLeaveRequestClaim? Find(string code);
    void Save(PrimaryLeaveRequestClaim item);
    bool Remove(string code);
    List<PrimaryLeaveRequestClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryLeaveRequestClaimStore(Dictionary<string, PrimaryLeaveRequestClaim> items, List<string> order) : IPrimaryLeaveRequestClaimStore
{
    public Dictionary<string, PrimaryLeaveRequestClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryLeaveRequestClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryLeaveRequestClaim item)
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

    public List<PrimaryLeaveRequestClaim> List()
    {
        var outValue = new List<PrimaryLeaveRequestClaim>();
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
public sealed class CachedPrimaryLeaveRequestClaimStore(MemoryPrimaryLeaveRequestClaimStore inner, PrimaryLeaveRequestClaimCache cache, int hits, int misses) : IPrimaryLeaveRequestClaimStore
{
    public MemoryPrimaryLeaveRequestClaimStore Inner { get; set; } = inner;
    public PrimaryLeaveRequestClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryLeaveRequestClaim? Find(string code)
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

    public void Save(PrimaryLeaveRequestClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryLeaveRequestClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary leave request claims. Empty fields match all.</summary>
public sealed record PrimaryLeaveRequestClaimQuery(string Name, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryLeaveRequestClaim item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<PrimaryLeaveRequestClaim> Apply(List<PrimaryLeaveRequestClaim> items)
    {
        var matched = Collections.Filter(items, (PrimaryLeaveRequestClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary leave request claim store.</summary>
public static class PrimaryLeaveRequestClaimStore
{
    public static IPrimaryLeaveRequestClaimStore NewMemoryPrimaryLeaveRequestClaimStore()
    {
        return new MemoryPrimaryLeaveRequestClaimStore(
            new Dictionary<string, PrimaryLeaveRequestClaim>(),
            new List<string>());
    }

    public static IPrimaryLeaveRequestClaimStore NewCachedPrimaryLeaveRequestClaimStore(MemoryPrimaryLeaveRequestClaimStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryLeaveRequestClaimStore(
            inner,
            PrimaryLeaveRequestClaimCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryLeaveRequestClaim> FindPrimaryLeaveRequestClaims(IPrimaryLeaveRequestClaimStore store, PrimaryLeaveRequestClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
