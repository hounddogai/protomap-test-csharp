namespace Acme.Workforce.Contractors.Engagement.EngagementProfile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional engagement notices by code.</summary>
public interface IRegionalEngagementNoticeStore
{
    RegionalEngagementNotice? Find(string code);
    void Save(RegionalEngagementNotice item);
    bool Remove(string code);
    List<RegionalEngagementNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalEngagementNoticeStore(Dictionary<string, RegionalEngagementNotice> items, List<string> order) : IRegionalEngagementNoticeStore
{
    public Dictionary<string, RegionalEngagementNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalEngagementNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalEngagementNotice item)
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

    public List<RegionalEngagementNotice> List()
    {
        var outValue = new List<RegionalEngagementNotice>();
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
public sealed class CachedRegionalEngagementNoticeStore(MemoryRegionalEngagementNoticeStore inner, RegionalEngagementNoticeCache cache, int hits, int misses) : IRegionalEngagementNoticeStore
{
    public MemoryRegionalEngagementNoticeStore Inner { get; set; } = inner;
    public RegionalEngagementNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalEngagementNotice? Find(string code)
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

    public void Save(RegionalEngagementNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalEngagementNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional engagement notices. Empty fields match all.</summary>
public sealed record RegionalEngagementNoticeQuery(string Title, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalEngagementNotice item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalEngagementNotice> Apply(List<RegionalEngagementNotice> items)
    {
        var matched = Collections.Filter(items, (RegionalEngagementNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional engagement notice store.</summary>
public static class RegionalEngagementNoticeStore
{
    public static IRegionalEngagementNoticeStore NewMemoryRegionalEngagementNoticeStore()
    {
        return new MemoryRegionalEngagementNoticeStore(
            new Dictionary<string, RegionalEngagementNotice>(),
            new List<string>());
    }

    public static IRegionalEngagementNoticeStore NewCachedRegionalEngagementNoticeStore(MemoryRegionalEngagementNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalEngagementNoticeStore(
            inner,
            RegionalEngagementNoticeCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RegionalEngagementNotice> FindRegionalEngagementNotices(IRegionalEngagementNoticeStore store, RegionalEngagementNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
