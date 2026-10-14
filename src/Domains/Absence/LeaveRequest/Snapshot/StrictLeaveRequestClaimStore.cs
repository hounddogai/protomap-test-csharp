namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict leave request claims by code.</summary>
public interface IStrictLeaveRequestClaimStore
{
    StrictLeaveRequestClaim? Find(string code);
    void Save(StrictLeaveRequestClaim item);
    bool Remove(string code);
    List<StrictLeaveRequestClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictLeaveRequestClaimStore(Dictionary<string, StrictLeaveRequestClaim> items, List<string> order) : IStrictLeaveRequestClaimStore
{
    public Dictionary<string, StrictLeaveRequestClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictLeaveRequestClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictLeaveRequestClaim item)
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

    public List<StrictLeaveRequestClaim> List()
    {
        var outValue = new List<StrictLeaveRequestClaim>();
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
public sealed class CachedStrictLeaveRequestClaimStore(MemoryStrictLeaveRequestClaimStore inner, StrictLeaveRequestClaimCache cache, int hits, int misses) : IStrictLeaveRequestClaimStore
{
    public MemoryStrictLeaveRequestClaimStore Inner { get; set; } = inner;
    public StrictLeaveRequestClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictLeaveRequestClaim? Find(string code)
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

    public void Save(StrictLeaveRequestClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictLeaveRequestClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict leave request claims. Empty fields match all.</summary>
public sealed record StrictLeaveRequestClaimQuery(string Label, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(StrictLeaveRequestClaim item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictLeaveRequestClaim> Apply(List<StrictLeaveRequestClaim> items)
    {
        var matched = Collections.Filter(items, (StrictLeaveRequestClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict leave request claim store.</summary>
public static class StrictLeaveRequestClaimStore
{
    public static IStrictLeaveRequestClaimStore NewMemoryStrictLeaveRequestClaimStore()
    {
        return new MemoryStrictLeaveRequestClaimStore(
            new Dictionary<string, StrictLeaveRequestClaim>(),
            new List<string>());
    }

    public static IStrictLeaveRequestClaimStore NewCachedStrictLeaveRequestClaimStore(MemoryStrictLeaveRequestClaimStore inner, int ttl, IClock clock)
    {
        return new CachedStrictLeaveRequestClaimStore(
            inner,
            StrictLeaveRequestClaimCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StrictLeaveRequestClaim> FindStrictLeaveRequestClaims(IStrictLeaveRequestClaimStore store, StrictLeaveRequestClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
