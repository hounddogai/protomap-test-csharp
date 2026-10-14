namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted visitor baselines by code.</summary>
public interface IWeightedVisitorBaselineStore
{
    WeightedVisitorBaseline? Find(string code);
    void Save(WeightedVisitorBaseline item);
    bool Remove(string code);
    List<WeightedVisitorBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedVisitorBaselineStore(Dictionary<string, WeightedVisitorBaseline> items, List<string> order) : IWeightedVisitorBaselineStore
{
    public Dictionary<string, WeightedVisitorBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedVisitorBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedVisitorBaseline item)
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

    public List<WeightedVisitorBaseline> List()
    {
        var outValue = new List<WeightedVisitorBaseline>();
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
public sealed class CachedWeightedVisitorBaselineStore(MemoryWeightedVisitorBaselineStore inner, WeightedVisitorBaselineCache cache, int hits, int misses) : IWeightedVisitorBaselineStore
{
    public MemoryWeightedVisitorBaselineStore Inner { get; set; } = inner;
    public WeightedVisitorBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedVisitorBaseline? Find(string code)
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

    public void Save(WeightedVisitorBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedVisitorBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted visitor baselines. Empty fields match all.</summary>
public sealed record WeightedVisitorBaselineQuery(string Segment, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedVisitorBaseline item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedVisitorBaseline> Apply(List<WeightedVisitorBaseline> items)
    {
        var matched = Collections.Filter(items, (WeightedVisitorBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted visitor baseline store.</summary>
public static class WeightedVisitorBaselineStore
{
    public static IWeightedVisitorBaselineStore NewMemoryWeightedVisitorBaselineStore()
    {
        return new MemoryWeightedVisitorBaselineStore(
            new Dictionary<string, WeightedVisitorBaseline>(),
            new List<string>());
    }

    public static IWeightedVisitorBaselineStore NewCachedWeightedVisitorBaselineStore(MemoryWeightedVisitorBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedVisitorBaselineStore(
            inner,
            WeightedVisitorBaselineCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<WeightedVisitorBaseline> FindWeightedVisitorBaselines(IWeightedVisitorBaselineStore store, WeightedVisitorBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
