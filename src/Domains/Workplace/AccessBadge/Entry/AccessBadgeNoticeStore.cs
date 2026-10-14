namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge notices by code.</summary>
public interface IAccessBadgeNoticeStore
{
    AccessBadgeNotice? Find(string code);
    void Save(AccessBadgeNotice item);
    bool Remove(string code);
    List<AccessBadgeNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeNoticeStore(Dictionary<string, AccessBadgeNotice> items, List<string> order) : IAccessBadgeNoticeStore
{
    public Dictionary<string, AccessBadgeNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeNotice item)
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

    public List<AccessBadgeNotice> List()
    {
        var outValue = new List<AccessBadgeNotice>();
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
public sealed class CachedAccessBadgeNoticeStore(MemoryAccessBadgeNoticeStore inner, AccessBadgeNoticeCache cache, int hits, int misses) : IAccessBadgeNoticeStore
{
    public MemoryAccessBadgeNoticeStore Inner { get; set; } = inner;
    public AccessBadgeNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeNotice? Find(string code)
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

    public void Save(AccessBadgeNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge notices. Empty fields match all.</summary>
public sealed record AccessBadgeNoticeQuery(string Title, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeNotice item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccessBadgeNotice> Apply(List<AccessBadgeNotice> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge notice store.</summary>
public static class AccessBadgeNoticeStore
{
    public static IAccessBadgeNoticeStore NewMemoryAccessBadgeNoticeStore()
    {
        return new MemoryAccessBadgeNoticeStore(new Dictionary<string, AccessBadgeNotice>(), new List<string>());
    }

    public static IAccessBadgeNoticeStore NewCachedAccessBadgeNoticeStore(MemoryAccessBadgeNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeNoticeStore(inner, AccessBadgeNoticeCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeNotice> FindAccessBadgeNotices(IAccessBadgeNoticeStore store, AccessBadgeNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
