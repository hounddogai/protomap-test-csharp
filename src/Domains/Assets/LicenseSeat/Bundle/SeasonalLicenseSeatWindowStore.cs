namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal license seat windows by code.</summary>
public interface ISeasonalLicenseSeatWindowStore
{
    SeasonalLicenseSeatWindow? Find(string code);
    void Save(SeasonalLicenseSeatWindow item);
    bool Remove(string code);
    List<SeasonalLicenseSeatWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalLicenseSeatWindowStore(Dictionary<string, SeasonalLicenseSeatWindow> items, List<string> order) : ISeasonalLicenseSeatWindowStore
{
    public Dictionary<string, SeasonalLicenseSeatWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalLicenseSeatWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalLicenseSeatWindow item)
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

    public List<SeasonalLicenseSeatWindow> List()
    {
        var outValue = new List<SeasonalLicenseSeatWindow>();
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
public sealed class CachedSeasonalLicenseSeatWindowStore(MemorySeasonalLicenseSeatWindowStore inner, SeasonalLicenseSeatWindowCache cache, int hits, int misses) : ISeasonalLicenseSeatWindowStore
{
    public MemorySeasonalLicenseSeatWindowStore Inner { get; set; } = inner;
    public SeasonalLicenseSeatWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalLicenseSeatWindow? Find(string code)
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

    public void Save(SeasonalLicenseSeatWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalLicenseSeatWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal license seat windows. Empty fields match all.</summary>
public sealed record SeasonalLicenseSeatWindowQuery(string Title, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalLicenseSeatWindow item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<SeasonalLicenseSeatWindow> Apply(List<SeasonalLicenseSeatWindow> items)
    {
        var matched = Collections.Filter(items, (SeasonalLicenseSeatWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal license seat window store.</summary>
public static class SeasonalLicenseSeatWindowStore
{
    public static ISeasonalLicenseSeatWindowStore NewMemorySeasonalLicenseSeatWindowStore()
    {
        return new MemorySeasonalLicenseSeatWindowStore(
            new Dictionary<string, SeasonalLicenseSeatWindow>(),
            new List<string>());
    }

    public static ISeasonalLicenseSeatWindowStore NewCachedSeasonalLicenseSeatWindowStore(MemorySeasonalLicenseSeatWindowStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalLicenseSeatWindowStore(
            inner,
            SeasonalLicenseSeatWindowCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalLicenseSeatWindow> FindSeasonalLicenseSeatWindows(ISeasonalLicenseSeatWindowStore store, SeasonalLicenseSeatWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
