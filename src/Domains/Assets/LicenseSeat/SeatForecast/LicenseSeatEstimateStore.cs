namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps license seat estimates by code.</summary>
public interface ILicenseSeatEstimateStore
{
    LicenseSeatEstimate? Find(string code);
    void Save(LicenseSeatEstimate item);
    bool Remove(string code);
    List<LicenseSeatEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLicenseSeatEstimateStore(Dictionary<string, LicenseSeatEstimate> items, List<string> order) : ILicenseSeatEstimateStore
{
    public Dictionary<string, LicenseSeatEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LicenseSeatEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LicenseSeatEstimate item)
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

    public List<LicenseSeatEstimate> List()
    {
        var outValue = new List<LicenseSeatEstimate>();
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
public sealed class CachedLicenseSeatEstimateStore(MemoryLicenseSeatEstimateStore inner, LicenseSeatEstimateCache cache, int hits, int misses) : ILicenseSeatEstimateStore
{
    public MemoryLicenseSeatEstimateStore Inner { get; set; } = inner;
    public LicenseSeatEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LicenseSeatEstimate? Find(string code)
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

    public void Save(LicenseSeatEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LicenseSeatEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects license seat estimates. Empty fields match all.</summary>
public sealed record LicenseSeatEstimateQuery(string Region, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(LicenseSeatEstimate item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LicenseSeatEstimate> Apply(List<LicenseSeatEstimate> items)
    {
        var matched = Collections.Filter(items, (LicenseSeatEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of license seat estimate store.</summary>
public static class LicenseSeatEstimateStore
{
    public static ILicenseSeatEstimateStore NewMemoryLicenseSeatEstimateStore()
    {
        return new MemoryLicenseSeatEstimateStore(new Dictionary<string, LicenseSeatEstimate>(), new List<string>());
    }

    public static ILicenseSeatEstimateStore NewCachedLicenseSeatEstimateStore(MemoryLicenseSeatEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedLicenseSeatEstimateStore(inner, LicenseSeatEstimateCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LicenseSeatEstimate> FindLicenseSeatEstimates(ILicenseSeatEstimateStore store, LicenseSeatEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
