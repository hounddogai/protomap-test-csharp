namespace Acme.Workforce.Engagement.TownHall.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall notices by code.</summary>
public interface ITownHallNoticeStore
{
    TownHallNotice? Find(string code);
    void Save(TownHallNotice item);
    bool Remove(string code);
    List<TownHallNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallNoticeStore(Dictionary<string, TownHallNotice> items, List<string> order) : ITownHallNoticeStore
{
    public Dictionary<string, TownHallNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallNotice item)
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

    public List<TownHallNotice> List()
    {
        var outValue = new List<TownHallNotice>();
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
public sealed class CachedTownHallNoticeStore(MemoryTownHallNoticeStore inner, TownHallNoticeCache cache, int hits, int misses) : ITownHallNoticeStore
{
    public MemoryTownHallNoticeStore Inner { get; set; } = inner;
    public TownHallNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallNotice? Find(string code)
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

    public void Save(TownHallNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall notices. Empty fields match all.</summary>
public sealed record TownHallNoticeQuery(string Name, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallNotice item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<TownHallNotice> Apply(List<TownHallNotice> items)
    {
        var matched = Collections.Filter(items, (TownHallNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall notice store.</summary>
public static class TownHallNoticeStore
{
    public static ITownHallNoticeStore NewMemoryTownHallNoticeStore()
    {
        return new MemoryTownHallNoticeStore(new Dictionary<string, TownHallNotice>(), new List<string>());
    }

    public static ITownHallNoticeStore NewCachedTownHallNoticeStore(MemoryTownHallNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallNoticeStore(inner, TownHallNoticeCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TownHallNotice> FindTownHallNotices(ITownHallNoticeStore store, TownHallNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
