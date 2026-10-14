namespace Acme.Workforce.Absence.Accrual.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled accrual overrides by code.</summary>
public interface IScheduledAccrualOverrideStore
{
    ScheduledAccrualOverride? Find(string code);
    void Save(ScheduledAccrualOverride item);
    bool Remove(string code);
    List<ScheduledAccrualOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledAccrualOverrideStore(Dictionary<string, ScheduledAccrualOverride> items, List<string> order) : IScheduledAccrualOverrideStore
{
    public Dictionary<string, ScheduledAccrualOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledAccrualOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledAccrualOverride item)
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

    public List<ScheduledAccrualOverride> List()
    {
        var outValue = new List<ScheduledAccrualOverride>();
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
public sealed class CachedScheduledAccrualOverrideStore(MemoryScheduledAccrualOverrideStore inner, ScheduledAccrualOverrideCache cache, int hits, int misses) : IScheduledAccrualOverrideStore
{
    public MemoryScheduledAccrualOverrideStore Inner { get; set; } = inner;
    public ScheduledAccrualOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledAccrualOverride? Find(string code)
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

    public void Save(ScheduledAccrualOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledAccrualOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled accrual overrides. Empty fields match all.</summary>
public sealed record ScheduledAccrualOverrideQuery(string Name, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledAccrualOverride item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledAccrualOverride> Apply(List<ScheduledAccrualOverride> items)
    {
        var matched = Collections.Filter(items, (ScheduledAccrualOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled accrual override store.</summary>
public static class ScheduledAccrualOverrideStore
{
    public static IScheduledAccrualOverrideStore NewMemoryScheduledAccrualOverrideStore()
    {
        return new MemoryScheduledAccrualOverrideStore(
            new Dictionary<string, ScheduledAccrualOverride>(),
            new List<string>());
    }

    public static IScheduledAccrualOverrideStore NewCachedScheduledAccrualOverrideStore(MemoryScheduledAccrualOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledAccrualOverrideStore(
            inner,
            ScheduledAccrualOverrideCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledAccrualOverride> FindScheduledAccrualOverrides(IScheduledAccrualOverrideStore store, ScheduledAccrualOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
