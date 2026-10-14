namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run adjustments by code.</summary>
public interface IPayRunAdjustmentStore
{
    PayRunAdjustment? Find(string code);
    void Save(PayRunAdjustment item);
    bool Remove(string code);
    List<PayRunAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunAdjustmentStore(Dictionary<string, PayRunAdjustment> items, List<string> order) : IPayRunAdjustmentStore
{
    public Dictionary<string, PayRunAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunAdjustment item)
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

    public List<PayRunAdjustment> List()
    {
        var outValue = new List<PayRunAdjustment>();
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
public sealed class CachedPayRunAdjustmentStore(MemoryPayRunAdjustmentStore inner, PayRunAdjustmentCache cache, int hits, int misses) : IPayRunAdjustmentStore
{
    public MemoryPayRunAdjustmentStore Inner { get; set; } = inner;
    public PayRunAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunAdjustment? Find(string code)
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

    public void Save(PayRunAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run adjustments. Empty fields match all.</summary>
public sealed record PayRunAdjustmentQuery(string Name, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunAdjustment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayRunAdjustment> Apply(List<PayRunAdjustment> items)
    {
        var matched = Collections.Filter(items, (PayRunAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run adjustment store.</summary>
public static class PayRunAdjustmentStore
{
    public static IPayRunAdjustmentStore NewMemoryPayRunAdjustmentStore()
    {
        return new MemoryPayRunAdjustmentStore(new Dictionary<string, PayRunAdjustment>(), new List<string>());
    }

    public static IPayRunAdjustmentStore NewCachedPayRunAdjustmentStore(MemoryPayRunAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunAdjustmentStore(inner, PayRunAdjustmentCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PayRunAdjustment> FindPayRunAdjustments(IPayRunAdjustmentStore store, PayRunAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
