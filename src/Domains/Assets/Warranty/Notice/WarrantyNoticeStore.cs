namespace Acme.Workforce.Assets.Warranty.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty notices by code.</summary>
public interface IWarrantyNoticeStore
{
    WarrantyNotice? Find(string code);
    void Save(WarrantyNotice item);
    bool Remove(string code);
    List<WarrantyNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyNoticeStore(Dictionary<string, WarrantyNotice> items, List<string> order) : IWarrantyNoticeStore
{
    public Dictionary<string, WarrantyNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyNotice item)
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

    public List<WarrantyNotice> List()
    {
        var outValue = new List<WarrantyNotice>();
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
public sealed class CachedWarrantyNoticeStore(MemoryWarrantyNoticeStore inner, WarrantyNoticeCache cache, int hits, int misses) : IWarrantyNoticeStore
{
    public MemoryWarrantyNoticeStore Inner { get; set; } = inner;
    public WarrantyNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyNotice? Find(string code)
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

    public void Save(WarrantyNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty notices. Empty fields match all.</summary>
public sealed record WarrantyNoticeQuery(string Region, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyNotice item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<WarrantyNotice> Apply(List<WarrantyNotice> items)
    {
        var matched = Collections.Filter(items, (WarrantyNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty notice store.</summary>
public static class WarrantyNoticeStore
{
    public static IWarrantyNoticeStore NewMemoryWarrantyNoticeStore()
    {
        return new MemoryWarrantyNoticeStore(new Dictionary<string, WarrantyNotice>(), new List<string>());
    }

    public static IWarrantyNoticeStore NewCachedWarrantyNoticeStore(MemoryWarrantyNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyNoticeStore(inner, WarrantyNoticeCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<WarrantyNotice> FindWarrantyNotices(IWarrantyNoticeStore store, WarrantyNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
