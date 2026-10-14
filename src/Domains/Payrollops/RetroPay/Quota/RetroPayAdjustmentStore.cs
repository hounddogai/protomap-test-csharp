namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay adjustments by code.</summary>
public interface IRetroPayAdjustmentStore
{
    RetroPayAdjustment? Find(string code);
    void Save(RetroPayAdjustment item);
    bool Remove(string code);
    List<RetroPayAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayAdjustmentStore(Dictionary<string, RetroPayAdjustment> items, List<string> order) : IRetroPayAdjustmentStore
{
    public Dictionary<string, RetroPayAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayAdjustment item)
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

    public List<RetroPayAdjustment> List()
    {
        var outValue = new List<RetroPayAdjustment>();
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
public sealed class CachedRetroPayAdjustmentStore(MemoryRetroPayAdjustmentStore inner, RetroPayAdjustmentCache cache, int hits, int misses) : IRetroPayAdjustmentStore
{
    public MemoryRetroPayAdjustmentStore Inner { get; set; } = inner;
    public RetroPayAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayAdjustment? Find(string code)
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

    public void Save(RetroPayAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay adjustments. Empty fields match all.</summary>
public sealed record RetroPayAdjustmentQuery(string Label, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayAdjustment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RetroPayAdjustment> Apply(List<RetroPayAdjustment> items)
    {
        var matched = Collections.Filter(items, (RetroPayAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay adjustment store.</summary>
public static class RetroPayAdjustmentStore
{
    public static IRetroPayAdjustmentStore NewMemoryRetroPayAdjustmentStore()
    {
        return new MemoryRetroPayAdjustmentStore(new Dictionary<string, RetroPayAdjustment>(), new List<string>());
    }

    public static IRetroPayAdjustmentStore NewCachedRetroPayAdjustmentStore(MemoryRetroPayAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayAdjustmentStore(inner, RetroPayAdjustmentCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RetroPayAdjustment> FindRetroPayAdjustments(IRetroPayAdjustmentStore store, RetroPayAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
