namespace Acme.Workforce.Contractors.Rate.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted rate notices by code.</summary>
public interface IWeightedRateNoticeStore
{
    WeightedRateNotice? Find(string code);
    void Save(WeightedRateNotice item);
    bool Remove(string code);
    List<WeightedRateNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedRateNoticeStore(Dictionary<string, WeightedRateNotice> items, List<string> order) : IWeightedRateNoticeStore
{
    public Dictionary<string, WeightedRateNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedRateNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedRateNotice item)
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

    public List<WeightedRateNotice> List()
    {
        var outValue = new List<WeightedRateNotice>();
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
public sealed class CachedWeightedRateNoticeStore(MemoryWeightedRateNoticeStore inner, WeightedRateNoticeCache cache, int hits, int misses) : IWeightedRateNoticeStore
{
    public MemoryWeightedRateNoticeStore Inner { get; set; } = inner;
    public WeightedRateNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedRateNotice? Find(string code)
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

    public void Save(WeightedRateNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedRateNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted rate notices. Empty fields match all.</summary>
public sealed record WeightedRateNoticeQuery(string Name, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedRateNotice item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedRateNotice> Apply(List<WeightedRateNotice> items)
    {
        var matched = Collections.Filter(items, (WeightedRateNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted rate notice store.</summary>
public static class WeightedRateNoticeStore
{
    public static IWeightedRateNoticeStore NewMemoryWeightedRateNoticeStore()
    {
        return new MemoryWeightedRateNoticeStore(new Dictionary<string, WeightedRateNotice>(), new List<string>());
    }

    public static IWeightedRateNoticeStore NewCachedWeightedRateNoticeStore(MemoryWeightedRateNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedRateNoticeStore(inner, WeightedRateNoticeCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<WeightedRateNotice> FindWeightedRateNotices(IWeightedRateNoticeStore store, WeightedRateNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
