namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback license seat windows by code.</summary>
public interface IFallbackLicenseSeatWindowStore
{
    FallbackLicenseSeatWindow? Find(string code);
    void Save(FallbackLicenseSeatWindow item);
    bool Remove(string code);
    List<FallbackLicenseSeatWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackLicenseSeatWindowStore(Dictionary<string, FallbackLicenseSeatWindow> items, List<string> order) : IFallbackLicenseSeatWindowStore
{
    public Dictionary<string, FallbackLicenseSeatWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackLicenseSeatWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackLicenseSeatWindow item)
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

    public List<FallbackLicenseSeatWindow> List()
    {
        var outValue = new List<FallbackLicenseSeatWindow>();
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
public sealed class CachedFallbackLicenseSeatWindowStore(MemoryFallbackLicenseSeatWindowStore inner, FallbackLicenseSeatWindowCache cache, int hits, int misses) : IFallbackLicenseSeatWindowStore
{
    public MemoryFallbackLicenseSeatWindowStore Inner { get; set; } = inner;
    public FallbackLicenseSeatWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackLicenseSeatWindow? Find(string code)
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

    public void Save(FallbackLicenseSeatWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackLicenseSeatWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback license seat windows. Empty fields match all.</summary>
public sealed record FallbackLicenseSeatWindowQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackLicenseSeatWindow item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackLicenseSeatWindow> Apply(List<FallbackLicenseSeatWindow> items)
    {
        var matched = Collections.Filter(items, (FallbackLicenseSeatWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback license seat window store.</summary>
public static class FallbackLicenseSeatWindowStore
{
    public static IFallbackLicenseSeatWindowStore NewMemoryFallbackLicenseSeatWindowStore()
    {
        return new MemoryFallbackLicenseSeatWindowStore(
            new Dictionary<string, FallbackLicenseSeatWindow>(),
            new List<string>());
    }

    public static IFallbackLicenseSeatWindowStore NewCachedFallbackLicenseSeatWindowStore(MemoryFallbackLicenseSeatWindowStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackLicenseSeatWindowStore(
            inner,
            FallbackLicenseSeatWindowCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<FallbackLicenseSeatWindow> FindFallbackLicenseSeatWindows(IFallbackLicenseSeatWindowStore store, FallbackLicenseSeatWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
