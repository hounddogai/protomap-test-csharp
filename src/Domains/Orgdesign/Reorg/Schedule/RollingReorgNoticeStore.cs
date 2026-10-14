namespace Acme.Workforce.Orgdesign.Reorg.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling reorg notices by code.</summary>
public interface IRollingReorgNoticeStore
{
    RollingReorgNotice? Find(string code);
    void Save(RollingReorgNotice item);
    bool Remove(string code);
    List<RollingReorgNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingReorgNoticeStore(Dictionary<string, RollingReorgNotice> items, List<string> order) : IRollingReorgNoticeStore
{
    public Dictionary<string, RollingReorgNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingReorgNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingReorgNotice item)
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

    public List<RollingReorgNotice> List()
    {
        var outValue = new List<RollingReorgNotice>();
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
public sealed class CachedRollingReorgNoticeStore(MemoryRollingReorgNoticeStore inner, RollingReorgNoticeCache cache, int hits, int misses) : IRollingReorgNoticeStore
{
    public MemoryRollingReorgNoticeStore Inner { get; set; } = inner;
    public RollingReorgNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingReorgNotice? Find(string code)
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

    public void Save(RollingReorgNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingReorgNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling reorg notices. Empty fields match all.</summary>
public sealed record RollingReorgNoticeQuery(string Region, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(RollingReorgNotice item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingReorgNotice> Apply(List<RollingReorgNotice> items)
    {
        var matched = Collections.Filter(items, (RollingReorgNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling reorg notice store.</summary>
public static class RollingReorgNoticeStore
{
    public static IRollingReorgNoticeStore NewMemoryRollingReorgNoticeStore()
    {
        return new MemoryRollingReorgNoticeStore(new Dictionary<string, RollingReorgNotice>(), new List<string>());
    }

    public static IRollingReorgNoticeStore NewCachedRollingReorgNoticeStore(MemoryRollingReorgNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedRollingReorgNoticeStore(inner, RollingReorgNoticeCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RollingReorgNotice> FindRollingReorgNotices(IRollingReorgNoticeStore store, RollingReorgNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
