namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg adjustments by code.</summary>
public interface IReorgAdjustmentStore
{
    ReorgAdjustment? Find(string code);
    void Save(ReorgAdjustment item);
    bool Remove(string code);
    List<ReorgAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgAdjustmentStore(Dictionary<string, ReorgAdjustment> items, List<string> order) : IReorgAdjustmentStore
{
    public Dictionary<string, ReorgAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgAdjustment item)
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

    public List<ReorgAdjustment> List()
    {
        var outValue = new List<ReorgAdjustment>();
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
public sealed class CachedReorgAdjustmentStore(MemoryReorgAdjustmentStore inner, ReorgAdjustmentCache cache, int hits, int misses) : IReorgAdjustmentStore
{
    public MemoryReorgAdjustmentStore Inner { get; set; } = inner;
    public ReorgAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgAdjustment? Find(string code)
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

    public void Save(ReorgAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg adjustments. Empty fields match all.</summary>
public sealed record ReorgAdjustmentQuery(string Label, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgAdjustment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReorgAdjustment> Apply(List<ReorgAdjustment> items)
    {
        var matched = Collections.Filter(items, (ReorgAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg adjustment store.</summary>
public static class ReorgAdjustmentStore
{
    public static IReorgAdjustmentStore NewMemoryReorgAdjustmentStore()
    {
        return new MemoryReorgAdjustmentStore(new Dictionary<string, ReorgAdjustment>(), new List<string>());
    }

    public static IReorgAdjustmentStore NewCachedReorgAdjustmentStore(MemoryReorgAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedReorgAdjustmentStore(inner, ReorgAdjustmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ReorgAdjustment> FindReorgAdjustments(IReorgAdjustmentStore store, ReorgAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
