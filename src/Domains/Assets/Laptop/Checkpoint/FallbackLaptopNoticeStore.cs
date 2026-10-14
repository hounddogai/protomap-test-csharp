namespace Acme.Workforce.Assets.Laptop.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback laptop notices by code.</summary>
public interface IFallbackLaptopNoticeStore
{
    FallbackLaptopNotice? Find(string code);
    void Save(FallbackLaptopNotice item);
    bool Remove(string code);
    List<FallbackLaptopNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackLaptopNoticeStore(Dictionary<string, FallbackLaptopNotice> items, List<string> order) : IFallbackLaptopNoticeStore
{
    public Dictionary<string, FallbackLaptopNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackLaptopNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackLaptopNotice item)
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

    public List<FallbackLaptopNotice> List()
    {
        var outValue = new List<FallbackLaptopNotice>();
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
public sealed class CachedFallbackLaptopNoticeStore(MemoryFallbackLaptopNoticeStore inner, FallbackLaptopNoticeCache cache, int hits, int misses) : IFallbackLaptopNoticeStore
{
    public MemoryFallbackLaptopNoticeStore Inner { get; set; } = inner;
    public FallbackLaptopNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackLaptopNotice? Find(string code)
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

    public void Save(FallbackLaptopNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackLaptopNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback laptop notices. Empty fields match all.</summary>
public sealed record FallbackLaptopNoticeQuery(string Department, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackLaptopNotice item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackLaptopNotice> Apply(List<FallbackLaptopNotice> items)
    {
        var matched = Collections.Filter(items, (FallbackLaptopNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback laptop notice store.</summary>
public static class FallbackLaptopNoticeStore
{
    public static IFallbackLaptopNoticeStore NewMemoryFallbackLaptopNoticeStore()
    {
        return new MemoryFallbackLaptopNoticeStore(
            new Dictionary<string, FallbackLaptopNotice>(),
            new List<string>());
    }

    public static IFallbackLaptopNoticeStore NewCachedFallbackLaptopNoticeStore(MemoryFallbackLaptopNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackLaptopNoticeStore(inner, FallbackLaptopNoticeCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<FallbackLaptopNotice> FindFallbackLaptopNotices(IFallbackLaptopNoticeStore store, FallbackLaptopNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
