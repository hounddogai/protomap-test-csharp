namespace Acme.Workforce.Contractors.Engagement.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled engagement notices by code.</summary>
public interface IScheduledEngagementNoticeStore
{
    ScheduledEngagementNotice? Find(string code);
    void Save(ScheduledEngagementNotice item);
    bool Remove(string code);
    List<ScheduledEngagementNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledEngagementNoticeStore(Dictionary<string, ScheduledEngagementNotice> items, List<string> order) : IScheduledEngagementNoticeStore
{
    public Dictionary<string, ScheduledEngagementNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledEngagementNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledEngagementNotice item)
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

    public List<ScheduledEngagementNotice> List()
    {
        var outValue = new List<ScheduledEngagementNotice>();
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
public sealed class CachedScheduledEngagementNoticeStore(MemoryScheduledEngagementNoticeStore inner, ScheduledEngagementNoticeCache cache, int hits, int misses) : IScheduledEngagementNoticeStore
{
    public MemoryScheduledEngagementNoticeStore Inner { get; set; } = inner;
    public ScheduledEngagementNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledEngagementNotice? Find(string code)
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

    public void Save(ScheduledEngagementNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledEngagementNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled engagement notices. Empty fields match all.</summary>
public sealed record ScheduledEngagementNoticeQuery(string Name, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledEngagementNotice item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledEngagementNotice> Apply(List<ScheduledEngagementNotice> items)
    {
        var matched = Collections.Filter(items, (ScheduledEngagementNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled engagement notice store.</summary>
public static class ScheduledEngagementNoticeStore
{
    public static IScheduledEngagementNoticeStore NewMemoryScheduledEngagementNoticeStore()
    {
        return new MemoryScheduledEngagementNoticeStore(
            new Dictionary<string, ScheduledEngagementNotice>(),
            new List<string>());
    }

    public static IScheduledEngagementNoticeStore NewCachedScheduledEngagementNoticeStore(MemoryScheduledEngagementNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledEngagementNoticeStore(
            inner,
            ScheduledEngagementNoticeCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledEngagementNotice> FindScheduledEngagementNotices(IScheduledEngagementNoticeStore store, ScheduledEngagementNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
