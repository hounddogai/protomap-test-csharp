namespace Acme.Workforce.Compensation.PayBand.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled pay band baselines by code.</summary>
public interface IScheduledPayBandBaselineStore
{
    ScheduledPayBandBaseline? Find(string code);
    void Save(ScheduledPayBandBaseline item);
    bool Remove(string code);
    List<ScheduledPayBandBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledPayBandBaselineStore(Dictionary<string, ScheduledPayBandBaseline> items, List<string> order) : IScheduledPayBandBaselineStore
{
    public Dictionary<string, ScheduledPayBandBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledPayBandBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledPayBandBaseline item)
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

    public List<ScheduledPayBandBaseline> List()
    {
        var outValue = new List<ScheduledPayBandBaseline>();
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
public sealed class CachedScheduledPayBandBaselineStore(MemoryScheduledPayBandBaselineStore inner, ScheduledPayBandBaselineCache cache, int hits, int misses) : IScheduledPayBandBaselineStore
{
    public MemoryScheduledPayBandBaselineStore Inner { get; set; } = inner;
    public ScheduledPayBandBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledPayBandBaseline? Find(string code)
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

    public void Save(ScheduledPayBandBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledPayBandBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled pay band baselines. Empty fields match all.</summary>
public sealed record ScheduledPayBandBaselineQuery(string Title, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledPayBandBaseline item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledPayBandBaseline> Apply(List<ScheduledPayBandBaseline> items)
    {
        var matched = Collections.Filter(items, (ScheduledPayBandBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled pay band baseline store.</summary>
public static class ScheduledPayBandBaselineStore
{
    public static IScheduledPayBandBaselineStore NewMemoryScheduledPayBandBaselineStore()
    {
        return new MemoryScheduledPayBandBaselineStore(
            new Dictionary<string, ScheduledPayBandBaseline>(),
            new List<string>());
    }

    public static IScheduledPayBandBaselineStore NewCachedScheduledPayBandBaselineStore(MemoryScheduledPayBandBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledPayBandBaselineStore(
            inner,
            ScheduledPayBandBaselineCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledPayBandBaseline> FindScheduledPayBandBaselines(IScheduledPayBandBaselineStore store, ScheduledPayBandBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
