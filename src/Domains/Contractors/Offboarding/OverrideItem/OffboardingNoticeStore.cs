namespace Acme.Workforce.Contractors.Offboarding.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding notices by code.</summary>
public interface IOffboardingNoticeStore
{
    OffboardingNotice? Find(string code);
    void Save(OffboardingNotice item);
    bool Remove(string code);
    List<OffboardingNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingNoticeStore(Dictionary<string, OffboardingNotice> items, List<string> order) : IOffboardingNoticeStore
{
    public Dictionary<string, OffboardingNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingNotice item)
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

    public List<OffboardingNotice> List()
    {
        var outValue = new List<OffboardingNotice>();
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
public sealed class CachedOffboardingNoticeStore(MemoryOffboardingNoticeStore inner, OffboardingNoticeCache cache, int hits, int misses) : IOffboardingNoticeStore
{
    public MemoryOffboardingNoticeStore Inner { get; set; } = inner;
    public OffboardingNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingNotice? Find(string code)
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

    public void Save(OffboardingNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding notices. Empty fields match all.</summary>
public sealed record OffboardingNoticeQuery(string Category, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingNotice item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<OffboardingNotice> Apply(List<OffboardingNotice> items)
    {
        var matched = Collections.Filter(items, (OffboardingNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding notice store.</summary>
public static class OffboardingNoticeStore
{
    public static IOffboardingNoticeStore NewMemoryOffboardingNoticeStore()
    {
        return new MemoryOffboardingNoticeStore(new Dictionary<string, OffboardingNotice>(), new List<string>());
    }

    public static IOffboardingNoticeStore NewCachedOffboardingNoticeStore(MemoryOffboardingNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingNoticeStore(inner, OffboardingNoticeCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<OffboardingNotice> FindOffboardingNotices(IOffboardingNoticeStore store, OffboardingNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
