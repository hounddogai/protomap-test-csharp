namespace Acme.Workforce.Talent.Pool.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool notices by code.</summary>
public interface IPoolNoticeStore
{
    PoolNotice? Find(string code);
    void Save(PoolNotice item);
    bool Remove(string code);
    List<PoolNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolNoticeStore(Dictionary<string, PoolNotice> items, List<string> order) : IPoolNoticeStore
{
    public Dictionary<string, PoolNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolNotice item)
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

    public List<PoolNotice> List()
    {
        var outValue = new List<PoolNotice>();
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
public sealed class CachedPoolNoticeStore(MemoryPoolNoticeStore inner, PoolNoticeCache cache, int hits, int misses) : IPoolNoticeStore
{
    public MemoryPoolNoticeStore Inner { get; set; } = inner;
    public PoolNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolNotice? Find(string code)
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

    public void Save(PoolNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool notices. Empty fields match all.</summary>
public sealed record PoolNoticeQuery(string Segment, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(PoolNotice item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolNotice> Apply(List<PoolNotice> items)
    {
        var matched = Collections.Filter(items, (PoolNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool notice store.</summary>
public static class PoolNoticeStore
{
    public static IPoolNoticeStore NewMemoryPoolNoticeStore()
    {
        return new MemoryPoolNoticeStore(new Dictionary<string, PoolNotice>(), new List<string>());
    }

    public static IPoolNoticeStore NewCachedPoolNoticeStore(MemoryPoolNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedPoolNoticeStore(inner, PoolNoticeCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PoolNotice> FindPoolNotices(IPoolNoticeStore store, PoolNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
