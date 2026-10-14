namespace Acme.Workforce.Absence.LeaveRequest.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request quotas by code.</summary>
public interface ILeaveRequestQuotaStore
{
    LeaveRequestQuota? Find(string code);
    void Save(LeaveRequestQuota item);
    bool Remove(string code);
    List<LeaveRequestQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestQuotaStore(Dictionary<string, LeaveRequestQuota> items, List<string> order) : ILeaveRequestQuotaStore
{
    public Dictionary<string, LeaveRequestQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestQuota item)
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

    public List<LeaveRequestQuota> List()
    {
        var outValue = new List<LeaveRequestQuota>();
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
public sealed class CachedLeaveRequestQuotaStore(MemoryLeaveRequestQuotaStore inner, LeaveRequestQuotaCache cache, int hits, int misses) : ILeaveRequestQuotaStore
{
    public MemoryLeaveRequestQuotaStore Inner { get; set; } = inner;
    public LeaveRequestQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestQuota? Find(string code)
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

    public void Save(LeaveRequestQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request quotas. Empty fields match all.</summary>
public sealed record LeaveRequestQuotaQuery(string Name, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestQuota item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LeaveRequestQuota> Apply(List<LeaveRequestQuota> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request quota store.</summary>
public static class LeaveRequestQuotaStore
{
    public static ILeaveRequestQuotaStore NewMemoryLeaveRequestQuotaStore()
    {
        return new MemoryLeaveRequestQuotaStore(new Dictionary<string, LeaveRequestQuota>(), new List<string>());
    }

    public static ILeaveRequestQuotaStore NewCachedLeaveRequestQuotaStore(MemoryLeaveRequestQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestQuotaStore(inner, LeaveRequestQuotaCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestQuota> FindLeaveRequestQuotas(ILeaveRequestQuotaStore store, LeaveRequestQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
