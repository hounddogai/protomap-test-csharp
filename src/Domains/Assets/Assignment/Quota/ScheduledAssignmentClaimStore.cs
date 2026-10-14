namespace Acme.Workforce.Assets.Assignment.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled assignment claims by code.</summary>
public interface IScheduledAssignmentClaimStore
{
    ScheduledAssignmentClaim? Find(string code);
    void Save(ScheduledAssignmentClaim item);
    bool Remove(string code);
    List<ScheduledAssignmentClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledAssignmentClaimStore(Dictionary<string, ScheduledAssignmentClaim> items, List<string> order) : IScheduledAssignmentClaimStore
{
    public Dictionary<string, ScheduledAssignmentClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledAssignmentClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledAssignmentClaim item)
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

    public List<ScheduledAssignmentClaim> List()
    {
        var outValue = new List<ScheduledAssignmentClaim>();
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
public sealed class CachedScheduledAssignmentClaimStore(MemoryScheduledAssignmentClaimStore inner, ScheduledAssignmentClaimCache cache, int hits, int misses) : IScheduledAssignmentClaimStore
{
    public MemoryScheduledAssignmentClaimStore Inner { get; set; } = inner;
    public ScheduledAssignmentClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledAssignmentClaim? Find(string code)
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

    public void Save(ScheduledAssignmentClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledAssignmentClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled assignment claims. Empty fields match all.</summary>
public sealed record ScheduledAssignmentClaimQuery(string Department, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledAssignmentClaim item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledAssignmentClaim> Apply(List<ScheduledAssignmentClaim> items)
    {
        var matched = Collections.Filter(items, (ScheduledAssignmentClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled assignment claim store.</summary>
public static class ScheduledAssignmentClaimStore
{
    public static IScheduledAssignmentClaimStore NewMemoryScheduledAssignmentClaimStore()
    {
        return new MemoryScheduledAssignmentClaimStore(
            new Dictionary<string, ScheduledAssignmentClaim>(),
            new List<string>());
    }

    public static IScheduledAssignmentClaimStore NewCachedScheduledAssignmentClaimStore(MemoryScheduledAssignmentClaimStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledAssignmentClaimStore(
            inner,
            ScheduledAssignmentClaimCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledAssignmentClaim> FindScheduledAssignmentClaims(IScheduledAssignmentClaimStore store, ScheduledAssignmentClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
