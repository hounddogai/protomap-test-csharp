namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor notices by code.</summary>
public interface IVisitorNoticeStore
{
    VisitorNotice? Find(string code);
    void Save(VisitorNotice item);
    bool Remove(string code);
    List<VisitorNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorNoticeStore(Dictionary<string, VisitorNotice> items, List<string> order) : IVisitorNoticeStore
{
    public Dictionary<string, VisitorNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorNotice item)
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

    public List<VisitorNotice> List()
    {
        var outValue = new List<VisitorNotice>();
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
public sealed class CachedVisitorNoticeStore(MemoryVisitorNoticeStore inner, VisitorNoticeCache cache, int hits, int misses) : IVisitorNoticeStore
{
    public MemoryVisitorNoticeStore Inner { get; set; } = inner;
    public VisitorNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorNotice? Find(string code)
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

    public void Save(VisitorNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor notices. Empty fields match all.</summary>
public sealed record VisitorNoticeQuery(string Country, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorNotice item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorNotice> Apply(List<VisitorNotice> items)
    {
        var matched = Collections.Filter(items, (VisitorNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor notice store.</summary>
public static class VisitorNoticeStore
{
    public static IVisitorNoticeStore NewMemoryVisitorNoticeStore()
    {
        return new MemoryVisitorNoticeStore(new Dictionary<string, VisitorNotice>(), new List<string>());
    }

    public static IVisitorNoticeStore NewCachedVisitorNoticeStore(MemoryVisitorNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorNoticeStore(inner, VisitorNoticeCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<VisitorNotice> FindVisitorNotices(IVisitorNoticeStore store, VisitorNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
