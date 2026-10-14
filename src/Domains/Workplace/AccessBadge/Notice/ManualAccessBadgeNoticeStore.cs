namespace Acme.Workforce.Workplace.AccessBadge.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual access badge notices by code.</summary>
public interface IManualAccessBadgeNoticeStore
{
    ManualAccessBadgeNotice? Find(string code);
    void Save(ManualAccessBadgeNotice item);
    bool Remove(string code);
    List<ManualAccessBadgeNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualAccessBadgeNoticeStore(Dictionary<string, ManualAccessBadgeNotice> items, List<string> order) : IManualAccessBadgeNoticeStore
{
    public Dictionary<string, ManualAccessBadgeNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualAccessBadgeNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualAccessBadgeNotice item)
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

    public List<ManualAccessBadgeNotice> List()
    {
        var outValue = new List<ManualAccessBadgeNotice>();
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
public sealed class CachedManualAccessBadgeNoticeStore(MemoryManualAccessBadgeNoticeStore inner, ManualAccessBadgeNoticeCache cache, int hits, int misses) : IManualAccessBadgeNoticeStore
{
    public MemoryManualAccessBadgeNoticeStore Inner { get; set; } = inner;
    public ManualAccessBadgeNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualAccessBadgeNotice? Find(string code)
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

    public void Save(ManualAccessBadgeNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualAccessBadgeNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual access badge notices. Empty fields match all.</summary>
public sealed record ManualAccessBadgeNoticeQuery(string Name, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(ManualAccessBadgeNotice item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualAccessBadgeNotice> Apply(List<ManualAccessBadgeNotice> items)
    {
        var matched = Collections.Filter(items, (ManualAccessBadgeNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual access badge notice store.</summary>
public static class ManualAccessBadgeNoticeStore
{
    public static IManualAccessBadgeNoticeStore NewMemoryManualAccessBadgeNoticeStore()
    {
        return new MemoryManualAccessBadgeNoticeStore(
            new Dictionary<string, ManualAccessBadgeNotice>(),
            new List<string>());
    }

    public static IManualAccessBadgeNoticeStore NewCachedManualAccessBadgeNoticeStore(MemoryManualAccessBadgeNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedManualAccessBadgeNoticeStore(
            inner,
            ManualAccessBadgeNoticeCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ManualAccessBadgeNotice> FindManualAccessBadgeNotices(IManualAccessBadgeNoticeStore store, ManualAccessBadgeNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
