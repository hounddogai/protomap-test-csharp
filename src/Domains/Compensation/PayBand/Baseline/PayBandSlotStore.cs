namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band slots by code.</summary>
public interface IPayBandSlotStore
{
    PayBandSlot? Find(string code);
    void Save(PayBandSlot item);
    bool Remove(string code);
    List<PayBandSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandSlotStore(Dictionary<string, PayBandSlot> items, List<string> order) : IPayBandSlotStore
{
    public Dictionary<string, PayBandSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandSlot item)
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

    public List<PayBandSlot> List()
    {
        var outValue = new List<PayBandSlot>();
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
public sealed class CachedPayBandSlotStore(MemoryPayBandSlotStore inner, PayBandSlotCache cache, int hits, int misses) : IPayBandSlotStore
{
    public MemoryPayBandSlotStore Inner { get; set; } = inner;
    public PayBandSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandSlot? Find(string code)
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

    public void Save(PayBandSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band slots. Empty fields match all.</summary>
public sealed record PayBandSlotQuery(string Source, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandSlot item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandSlot> Apply(List<PayBandSlot> items)
    {
        var matched = Collections.Filter(items, (PayBandSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band slot store.</summary>
public static class PayBandSlotStore
{
    public static IPayBandSlotStore NewMemoryPayBandSlotStore()
    {
        return new MemoryPayBandSlotStore(new Dictionary<string, PayBandSlot>(), new List<string>());
    }

    public static IPayBandSlotStore NewCachedPayBandSlotStore(MemoryPayBandSlotStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandSlotStore(inner, PayBandSlotCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PayBandSlot> FindPayBandSlots(IPayBandSlotStore store, PayBandSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
