namespace Acme.Workforce.Absence.LeaveRequest.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request digests by code.</summary>
public interface ILeaveRequestDigestStore
{
    LeaveRequestDigest? Find(string code);
    void Save(LeaveRequestDigest item);
    bool Remove(string code);
    List<LeaveRequestDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestDigestStore(Dictionary<string, LeaveRequestDigest> items, List<string> order) : ILeaveRequestDigestStore
{
    public Dictionary<string, LeaveRequestDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestDigest item)
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

    public List<LeaveRequestDigest> List()
    {
        var outValue = new List<LeaveRequestDigest>();
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
public sealed class CachedLeaveRequestDigestStore(MemoryLeaveRequestDigestStore inner, LeaveRequestDigestCache cache, int hits, int misses) : ILeaveRequestDigestStore
{
    public MemoryLeaveRequestDigestStore Inner { get; set; } = inner;
    public LeaveRequestDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestDigest? Find(string code)
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

    public void Save(LeaveRequestDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request digests. Empty fields match all.</summary>
public sealed record LeaveRequestDigestQuery(string Region, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestDigest item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<LeaveRequestDigest> Apply(List<LeaveRequestDigest> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request digest store.</summary>
public static class LeaveRequestDigestStore
{
    public static ILeaveRequestDigestStore NewMemoryLeaveRequestDigestStore()
    {
        return new MemoryLeaveRequestDigestStore(new Dictionary<string, LeaveRequestDigest>(), new List<string>());
    }

    public static ILeaveRequestDigestStore NewCachedLeaveRequestDigestStore(MemoryLeaveRequestDigestStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestDigestStore(inner, LeaveRequestDigestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestDigest> FindLeaveRequestDigests(ILeaveRequestDigestStore store, LeaveRequestDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
