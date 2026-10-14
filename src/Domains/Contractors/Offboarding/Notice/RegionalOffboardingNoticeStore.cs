namespace Acme.Workforce.Contractors.Offboarding.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional offboarding notices by code.</summary>
public interface IRegionalOffboardingNoticeStore
{
    RegionalOffboardingNotice? Find(string code);
    void Save(RegionalOffboardingNotice item);
    bool Remove(string code);
    List<RegionalOffboardingNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalOffboardingNoticeStore(Dictionary<string, RegionalOffboardingNotice> items, List<string> order) : IRegionalOffboardingNoticeStore
{
    public Dictionary<string, RegionalOffboardingNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalOffboardingNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalOffboardingNotice item)
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

    public List<RegionalOffboardingNotice> List()
    {
        var outValue = new List<RegionalOffboardingNotice>();
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
public sealed class CachedRegionalOffboardingNoticeStore(MemoryRegionalOffboardingNoticeStore inner, RegionalOffboardingNoticeCache cache, int hits, int misses) : IRegionalOffboardingNoticeStore
{
    public MemoryRegionalOffboardingNoticeStore Inner { get; set; } = inner;
    public RegionalOffboardingNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalOffboardingNotice? Find(string code)
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

    public void Save(RegionalOffboardingNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalOffboardingNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional offboarding notices. Empty fields match all.</summary>
public sealed record RegionalOffboardingNoticeQuery(string Unit, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalOffboardingNotice item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalOffboardingNotice> Apply(List<RegionalOffboardingNotice> items)
    {
        var matched = Collections.Filter(items, (RegionalOffboardingNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional offboarding notice store.</summary>
public static class RegionalOffboardingNoticeStore
{
    public static IRegionalOffboardingNoticeStore NewMemoryRegionalOffboardingNoticeStore()
    {
        return new MemoryRegionalOffboardingNoticeStore(
            new Dictionary<string, RegionalOffboardingNotice>(),
            new List<string>());
    }

    public static IRegionalOffboardingNoticeStore NewCachedRegionalOffboardingNoticeStore(MemoryRegionalOffboardingNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalOffboardingNoticeStore(
            inner,
            RegionalOffboardingNoticeCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RegionalOffboardingNotice> FindRegionalOffboardingNotices(IRegionalOffboardingNoticeStore store, RegionalOffboardingNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
