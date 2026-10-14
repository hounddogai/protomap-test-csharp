namespace Acme.Workforce.Contractors.Engagement.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive engagement notices by code.</summary>
public interface IAdaptiveEngagementNoticeStore
{
    AdaptiveEngagementNotice? Find(string code);
    void Save(AdaptiveEngagementNotice item);
    bool Remove(string code);
    List<AdaptiveEngagementNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveEngagementNoticeStore(Dictionary<string, AdaptiveEngagementNotice> items, List<string> order) : IAdaptiveEngagementNoticeStore
{
    public Dictionary<string, AdaptiveEngagementNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveEngagementNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveEngagementNotice item)
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

    public List<AdaptiveEngagementNotice> List()
    {
        var outValue = new List<AdaptiveEngagementNotice>();
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
public sealed class CachedAdaptiveEngagementNoticeStore(MemoryAdaptiveEngagementNoticeStore inner, AdaptiveEngagementNoticeCache cache, int hits, int misses) : IAdaptiveEngagementNoticeStore
{
    public MemoryAdaptiveEngagementNoticeStore Inner { get; set; } = inner;
    public AdaptiveEngagementNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveEngagementNotice? Find(string code)
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

    public void Save(AdaptiveEngagementNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveEngagementNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive engagement notices. Empty fields match all.</summary>
public sealed record AdaptiveEngagementNoticeQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveEngagementNotice item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveEngagementNotice> Apply(List<AdaptiveEngagementNotice> items)
    {
        var matched = Collections.Filter(items, (AdaptiveEngagementNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive engagement notice store.</summary>
public static class AdaptiveEngagementNoticeStore
{
    public static IAdaptiveEngagementNoticeStore NewMemoryAdaptiveEngagementNoticeStore()
    {
        return new MemoryAdaptiveEngagementNoticeStore(
            new Dictionary<string, AdaptiveEngagementNotice>(),
            new List<string>());
    }

    public static IAdaptiveEngagementNoticeStore NewCachedAdaptiveEngagementNoticeStore(MemoryAdaptiveEngagementNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveEngagementNoticeStore(
            inner,
            AdaptiveEngagementNoticeCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveEngagementNotice> FindAdaptiveEngagementNotices(IAdaptiveEngagementNoticeStore store, AdaptiveEngagementNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
