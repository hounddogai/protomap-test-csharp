namespace Acme.Workforce.Payrollops.RetroPay.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted retro pay notices by code.</summary>
public interface IWeightedRetroPayNoticeStore
{
    WeightedRetroPayNotice? Find(string code);
    void Save(WeightedRetroPayNotice item);
    bool Remove(string code);
    List<WeightedRetroPayNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedRetroPayNoticeStore(Dictionary<string, WeightedRetroPayNotice> items, List<string> order) : IWeightedRetroPayNoticeStore
{
    public Dictionary<string, WeightedRetroPayNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedRetroPayNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedRetroPayNotice item)
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

    public List<WeightedRetroPayNotice> List()
    {
        var outValue = new List<WeightedRetroPayNotice>();
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
public sealed class CachedWeightedRetroPayNoticeStore(MemoryWeightedRetroPayNoticeStore inner, WeightedRetroPayNoticeCache cache, int hits, int misses) : IWeightedRetroPayNoticeStore
{
    public MemoryWeightedRetroPayNoticeStore Inner { get; set; } = inner;
    public WeightedRetroPayNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedRetroPayNotice? Find(string code)
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

    public void Save(WeightedRetroPayNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedRetroPayNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted retro pay notices. Empty fields match all.</summary>
public sealed record WeightedRetroPayNoticeQuery(string Unit, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedRetroPayNotice item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedRetroPayNotice> Apply(List<WeightedRetroPayNotice> items)
    {
        var matched = Collections.Filter(items, (WeightedRetroPayNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted retro pay notice store.</summary>
public static class WeightedRetroPayNoticeStore
{
    public static IWeightedRetroPayNoticeStore NewMemoryWeightedRetroPayNoticeStore()
    {
        return new MemoryWeightedRetroPayNoticeStore(
            new Dictionary<string, WeightedRetroPayNotice>(),
            new List<string>());
    }

    public static IWeightedRetroPayNoticeStore NewCachedWeightedRetroPayNoticeStore(MemoryWeightedRetroPayNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedRetroPayNoticeStore(
            inner,
            WeightedRetroPayNoticeCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<WeightedRetroPayNotice> FindWeightedRetroPayNotices(IWeightedRetroPayNoticeStore store, WeightedRetroPayNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
