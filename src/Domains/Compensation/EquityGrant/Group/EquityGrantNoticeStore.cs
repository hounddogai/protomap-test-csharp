namespace Acme.Workforce.Compensation.EquityGrant.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant notices by code.</summary>
public interface IEquityGrantNoticeStore
{
    EquityGrantNotice? Find(string code);
    void Save(EquityGrantNotice item);
    bool Remove(string code);
    List<EquityGrantNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantNoticeStore(Dictionary<string, EquityGrantNotice> items, List<string> order) : IEquityGrantNoticeStore
{
    public Dictionary<string, EquityGrantNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantNotice item)
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

    public List<EquityGrantNotice> List()
    {
        var outValue = new List<EquityGrantNotice>();
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
public sealed class CachedEquityGrantNoticeStore(MemoryEquityGrantNoticeStore inner, EquityGrantNoticeCache cache, int hits, int misses) : IEquityGrantNoticeStore
{
    public MemoryEquityGrantNoticeStore Inner { get; set; } = inner;
    public EquityGrantNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantNotice? Find(string code)
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

    public void Save(EquityGrantNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant notices. Empty fields match all.</summary>
public sealed record EquityGrantNoticeQuery(string Title, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantNotice item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantNotice> Apply(List<EquityGrantNotice> items)
    {
        var matched = Collections.Filter(items, (EquityGrantNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant notice store.</summary>
public static class EquityGrantNoticeStore
{
    public static IEquityGrantNoticeStore NewMemoryEquityGrantNoticeStore()
    {
        return new MemoryEquityGrantNoticeStore(new Dictionary<string, EquityGrantNotice>(), new List<string>());
    }

    public static IEquityGrantNoticeStore NewCachedEquityGrantNoticeStore(MemoryEquityGrantNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantNoticeStore(inner, EquityGrantNoticeCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<EquityGrantNotice> FindEquityGrantNotices(IEquityGrantNoticeStore store, EquityGrantNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
