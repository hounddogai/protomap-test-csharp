namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request claims by code.</summary>
public interface ILeaveRequestClaimStore
{
    LeaveRequestClaim? Find(string code);
    void Save(LeaveRequestClaim item);
    bool Remove(string code);
    List<LeaveRequestClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestClaimStore(Dictionary<string, LeaveRequestClaim> items, List<string> order) : ILeaveRequestClaimStore
{
    public Dictionary<string, LeaveRequestClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestClaim item)
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

    public List<LeaveRequestClaim> List()
    {
        var outValue = new List<LeaveRequestClaim>();
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
public sealed class CachedLeaveRequestClaimStore(MemoryLeaveRequestClaimStore inner, LeaveRequestClaimCache cache, int hits, int misses) : ILeaveRequestClaimStore
{
    public MemoryLeaveRequestClaimStore Inner { get; set; } = inner;
    public LeaveRequestClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestClaim? Find(string code)
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

    public void Save(LeaveRequestClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request claims. Empty fields match all.</summary>
public sealed record LeaveRequestClaimQuery(string Title, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestClaim item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LeaveRequestClaim> Apply(List<LeaveRequestClaim> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request claim store.</summary>
public static class LeaveRequestClaimStore
{
    public static ILeaveRequestClaimStore NewMemoryLeaveRequestClaimStore()
    {
        return new MemoryLeaveRequestClaimStore(new Dictionary<string, LeaveRequestClaim>(), new List<string>());
    }

    public static ILeaveRequestClaimStore NewCachedLeaveRequestClaimStore(MemoryLeaveRequestClaimStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestClaimStore(inner, LeaveRequestClaimCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestClaim> FindLeaveRequestClaims(ILeaveRequestClaimStore store, LeaveRequestClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
