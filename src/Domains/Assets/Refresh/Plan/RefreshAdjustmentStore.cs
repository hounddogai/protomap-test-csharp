namespace Acme.Workforce.Assets.Refresh.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh adjustments by code.</summary>
public interface IRefreshAdjustmentStore
{
    RefreshAdjustment? Find(string code);
    void Save(RefreshAdjustment item);
    bool Remove(string code);
    List<RefreshAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshAdjustmentStore(Dictionary<string, RefreshAdjustment> items, List<string> order) : IRefreshAdjustmentStore
{
    public Dictionary<string, RefreshAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshAdjustment item)
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

    public List<RefreshAdjustment> List()
    {
        var outValue = new List<RefreshAdjustment>();
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
public sealed class CachedRefreshAdjustmentStore(MemoryRefreshAdjustmentStore inner, RefreshAdjustmentCache cache, int hits, int misses) : IRefreshAdjustmentStore
{
    public MemoryRefreshAdjustmentStore Inner { get; set; } = inner;
    public RefreshAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshAdjustment? Find(string code)
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

    public void Save(RefreshAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh adjustments. Empty fields match all.</summary>
public sealed record RefreshAdjustmentQuery(string Source, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshAdjustment item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<RefreshAdjustment> Apply(List<RefreshAdjustment> items)
    {
        var matched = Collections.Filter(items, (RefreshAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh adjustment store.</summary>
public static class RefreshAdjustmentStore
{
    public static IRefreshAdjustmentStore NewMemoryRefreshAdjustmentStore()
    {
        return new MemoryRefreshAdjustmentStore(new Dictionary<string, RefreshAdjustment>(), new List<string>());
    }

    public static IRefreshAdjustmentStore NewCachedRefreshAdjustmentStore(MemoryRefreshAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshAdjustmentStore(inner, RefreshAdjustmentCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RefreshAdjustment> FindRefreshAdjustments(IRefreshAdjustmentStore store, RefreshAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
