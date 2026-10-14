namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled leave request notices by code.</summary>
public interface IScheduledLeaveRequestNoticeStore
{
    ScheduledLeaveRequestNotice? Find(string code);
    void Save(ScheduledLeaveRequestNotice item);
    bool Remove(string code);
    List<ScheduledLeaveRequestNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledLeaveRequestNoticeStore(Dictionary<string, ScheduledLeaveRequestNotice> items, List<string> order) : IScheduledLeaveRequestNoticeStore
{
    public Dictionary<string, ScheduledLeaveRequestNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledLeaveRequestNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledLeaveRequestNotice item)
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

    public List<ScheduledLeaveRequestNotice> List()
    {
        var outValue = new List<ScheduledLeaveRequestNotice>();
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
public sealed class CachedScheduledLeaveRequestNoticeStore(MemoryScheduledLeaveRequestNoticeStore inner, ScheduledLeaveRequestNoticeCache cache, int hits, int misses) : IScheduledLeaveRequestNoticeStore
{
    public MemoryScheduledLeaveRequestNoticeStore Inner { get; set; } = inner;
    public ScheduledLeaveRequestNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledLeaveRequestNotice? Find(string code)
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

    public void Save(ScheduledLeaveRequestNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledLeaveRequestNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled leave request notices. Empty fields match all.</summary>
public sealed record ScheduledLeaveRequestNoticeQuery(string Channel, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledLeaveRequestNotice item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<ScheduledLeaveRequestNotice> Apply(List<ScheduledLeaveRequestNotice> items)
    {
        var matched = Collections.Filter(items, (ScheduledLeaveRequestNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled leave request notice store.</summary>
public static class ScheduledLeaveRequestNoticeStore
{
    public static IScheduledLeaveRequestNoticeStore NewMemoryScheduledLeaveRequestNoticeStore()
    {
        return new MemoryScheduledLeaveRequestNoticeStore(
            new Dictionary<string, ScheduledLeaveRequestNotice>(),
            new List<string>());
    }

    public static IScheduledLeaveRequestNoticeStore NewCachedScheduledLeaveRequestNoticeStore(MemoryScheduledLeaveRequestNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledLeaveRequestNoticeStore(
            inner,
            ScheduledLeaveRequestNoticeCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledLeaveRequestNotice> FindScheduledLeaveRequestNotices(IScheduledLeaveRequestNoticeStore store, ScheduledLeaveRequestNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
