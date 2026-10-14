namespace Acme.Workforce.Assets.Laptop.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop notices by code.</summary>
public interface ILaptopNoticeStore
{
    LaptopNotice? Find(string code);
    void Save(LaptopNotice item);
    bool Remove(string code);
    List<LaptopNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopNoticeStore(Dictionary<string, LaptopNotice> items, List<string> order) : ILaptopNoticeStore
{
    public Dictionary<string, LaptopNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopNotice item)
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

    public List<LaptopNotice> List()
    {
        var outValue = new List<LaptopNotice>();
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
public sealed class CachedLaptopNoticeStore(MemoryLaptopNoticeStore inner, LaptopNoticeCache cache, int hits, int misses) : ILaptopNoticeStore
{
    public MemoryLaptopNoticeStore Inner { get; set; } = inner;
    public LaptopNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopNotice? Find(string code)
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

    public void Save(LaptopNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop notices. Empty fields match all.</summary>
public sealed record LaptopNoticeQuery(string Label, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopNotice item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopNotice> Apply(List<LaptopNotice> items)
    {
        var matched = Collections.Filter(items, (LaptopNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop notice store.</summary>
public static class LaptopNoticeStore
{
    public static ILaptopNoticeStore NewMemoryLaptopNoticeStore()
    {
        return new MemoryLaptopNoticeStore(new Dictionary<string, LaptopNotice>(), new List<string>());
    }

    public static ILaptopNoticeStore NewCachedLaptopNoticeStore(MemoryLaptopNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopNoticeStore(inner, LaptopNoticeCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LaptopNotice> FindLaptopNotices(ILaptopNoticeStore store, LaptopNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
