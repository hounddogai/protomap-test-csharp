namespace Acme.Workforce.Absence.Accrual.AccrualTier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled accrual baselines by code.</summary>
public interface IScheduledAccrualBaselineStore
{
    ScheduledAccrualBaseline? Find(string code);
    void Save(ScheduledAccrualBaseline item);
    bool Remove(string code);
    List<ScheduledAccrualBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledAccrualBaselineStore(Dictionary<string, ScheduledAccrualBaseline> items, List<string> order) : IScheduledAccrualBaselineStore
{
    public Dictionary<string, ScheduledAccrualBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledAccrualBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledAccrualBaseline item)
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

    public List<ScheduledAccrualBaseline> List()
    {
        var outValue = new List<ScheduledAccrualBaseline>();
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
public sealed class CachedScheduledAccrualBaselineStore(MemoryScheduledAccrualBaselineStore inner, ScheduledAccrualBaselineCache cache, int hits, int misses) : IScheduledAccrualBaselineStore
{
    public MemoryScheduledAccrualBaselineStore Inner { get; set; } = inner;
    public ScheduledAccrualBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledAccrualBaseline? Find(string code)
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

    public void Save(ScheduledAccrualBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledAccrualBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled accrual baselines. Empty fields match all.</summary>
public sealed record ScheduledAccrualBaselineQuery(string Channel, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledAccrualBaseline item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<ScheduledAccrualBaseline> Apply(List<ScheduledAccrualBaseline> items)
    {
        var matched = Collections.Filter(items, (ScheduledAccrualBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled accrual baseline store.</summary>
public static class ScheduledAccrualBaselineStore
{
    public static IScheduledAccrualBaselineStore NewMemoryScheduledAccrualBaselineStore()
    {
        return new MemoryScheduledAccrualBaselineStore(
            new Dictionary<string, ScheduledAccrualBaseline>(),
            new List<string>());
    }

    public static IScheduledAccrualBaselineStore NewCachedScheduledAccrualBaselineStore(MemoryScheduledAccrualBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledAccrualBaselineStore(
            inner,
            ScheduledAccrualBaselineCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledAccrualBaseline> FindScheduledAccrualBaselines(IScheduledAccrualBaselineStore store, ScheduledAccrualBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
