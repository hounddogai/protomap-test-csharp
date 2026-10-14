namespace Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered leave request overrides by code.</summary>
public interface ITieredLeaveRequestOverrideStore
{
    TieredLeaveRequestOverride? Find(string code);
    void Save(TieredLeaveRequestOverride item);
    bool Remove(string code);
    List<TieredLeaveRequestOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredLeaveRequestOverrideStore(Dictionary<string, TieredLeaveRequestOverride> items, List<string> order) : ITieredLeaveRequestOverrideStore
{
    public Dictionary<string, TieredLeaveRequestOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredLeaveRequestOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredLeaveRequestOverride item)
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

    public List<TieredLeaveRequestOverride> List()
    {
        var outValue = new List<TieredLeaveRequestOverride>();
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
public sealed class CachedTieredLeaveRequestOverrideStore(MemoryTieredLeaveRequestOverrideStore inner, TieredLeaveRequestOverrideCache cache, int hits, int misses) : ITieredLeaveRequestOverrideStore
{
    public MemoryTieredLeaveRequestOverrideStore Inner { get; set; } = inner;
    public TieredLeaveRequestOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredLeaveRequestOverride? Find(string code)
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

    public void Save(TieredLeaveRequestOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredLeaveRequestOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered leave request overrides. Empty fields match all.</summary>
public sealed record TieredLeaveRequestOverrideQuery(string Team, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(TieredLeaveRequestOverride item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<TieredLeaveRequestOverride> Apply(List<TieredLeaveRequestOverride> items)
    {
        var matched = Collections.Filter(items, (TieredLeaveRequestOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered leave request override store.</summary>
public static class TieredLeaveRequestOverrideStore
{
    public static ITieredLeaveRequestOverrideStore NewMemoryTieredLeaveRequestOverrideStore()
    {
        return new MemoryTieredLeaveRequestOverrideStore(
            new Dictionary<string, TieredLeaveRequestOverride>(),
            new List<string>());
    }

    public static ITieredLeaveRequestOverrideStore NewCachedTieredLeaveRequestOverrideStore(MemoryTieredLeaveRequestOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedTieredLeaveRequestOverrideStore(
            inner,
            TieredLeaveRequestOverrideCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<TieredLeaveRequestOverride> FindTieredLeaveRequestOverrides(ITieredLeaveRequestOverrideStore store, TieredLeaveRequestOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
