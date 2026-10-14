namespace Acme.Workforce.Assets.LicenseSeat.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps license seat notices by code.</summary>
public interface ILicenseSeatNoticeStore
{
    LicenseSeatNotice? Find(string code);
    void Save(LicenseSeatNotice item);
    bool Remove(string code);
    List<LicenseSeatNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLicenseSeatNoticeStore(Dictionary<string, LicenseSeatNotice> items, List<string> order) : ILicenseSeatNoticeStore
{
    public Dictionary<string, LicenseSeatNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LicenseSeatNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LicenseSeatNotice item)
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

    public List<LicenseSeatNotice> List()
    {
        var outValue = new List<LicenseSeatNotice>();
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
public sealed class CachedLicenseSeatNoticeStore(MemoryLicenseSeatNoticeStore inner, LicenseSeatNoticeCache cache, int hits, int misses) : ILicenseSeatNoticeStore
{
    public MemoryLicenseSeatNoticeStore Inner { get; set; } = inner;
    public LicenseSeatNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LicenseSeatNotice? Find(string code)
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

    public void Save(LicenseSeatNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LicenseSeatNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects license seat notices. Empty fields match all.</summary>
public sealed record LicenseSeatNoticeQuery(string Category, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(LicenseSeatNotice item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LicenseSeatNotice> Apply(List<LicenseSeatNotice> items)
    {
        var matched = Collections.Filter(items, (LicenseSeatNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of license seat notice store.</summary>
public static class LicenseSeatNoticeStore
{
    public static ILicenseSeatNoticeStore NewMemoryLicenseSeatNoticeStore()
    {
        return new MemoryLicenseSeatNoticeStore(new Dictionary<string, LicenseSeatNotice>(), new List<string>());
    }

    public static ILicenseSeatNoticeStore NewCachedLicenseSeatNoticeStore(MemoryLicenseSeatNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedLicenseSeatNoticeStore(inner, LicenseSeatNoticeCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LicenseSeatNotice> FindLicenseSeatNotices(ILicenseSeatNoticeStore store, LicenseSeatNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
