namespace Acme.Workforce.Assets.Laptop.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered laptop notices by code.</summary>
public interface ITieredLaptopNoticeStore
{
    TieredLaptopNotice? Find(string code);
    void Save(TieredLaptopNotice item);
    bool Remove(string code);
    List<TieredLaptopNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredLaptopNoticeStore(Dictionary<string, TieredLaptopNotice> items, List<string> order) : ITieredLaptopNoticeStore
{
    public Dictionary<string, TieredLaptopNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredLaptopNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredLaptopNotice item)
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

    public List<TieredLaptopNotice> List()
    {
        var outValue = new List<TieredLaptopNotice>();
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
public sealed class CachedTieredLaptopNoticeStore(MemoryTieredLaptopNoticeStore inner, TieredLaptopNoticeCache cache, int hits, int misses) : ITieredLaptopNoticeStore
{
    public MemoryTieredLaptopNoticeStore Inner { get; set; } = inner;
    public TieredLaptopNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredLaptopNotice? Find(string code)
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

    public void Save(TieredLaptopNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredLaptopNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered laptop notices. Empty fields match all.</summary>
public sealed record TieredLaptopNoticeQuery(string Title, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(TieredLaptopNotice item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredLaptopNotice> Apply(List<TieredLaptopNotice> items)
    {
        var matched = Collections.Filter(items, (TieredLaptopNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered laptop notice store.</summary>
public static class TieredLaptopNoticeStore
{
    public static ITieredLaptopNoticeStore NewMemoryTieredLaptopNoticeStore()
    {
        return new MemoryTieredLaptopNoticeStore(new Dictionary<string, TieredLaptopNotice>(), new List<string>());
    }

    public static ITieredLaptopNoticeStore NewCachedTieredLaptopNoticeStore(MemoryTieredLaptopNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedTieredLaptopNoticeStore(inner, TieredLaptopNoticeCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TieredLaptopNotice> FindTieredLaptopNotices(ITieredLaptopNoticeStore store, TieredLaptopNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
