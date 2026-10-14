namespace Acme.Workforce.Assets.Refresh.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal refresh windows by code.</summary>
public interface ISeasonalRefreshWindowStore
{
    SeasonalRefreshWindow? Find(string code);
    void Save(SeasonalRefreshWindow item);
    bool Remove(string code);
    List<SeasonalRefreshWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalRefreshWindowStore(Dictionary<string, SeasonalRefreshWindow> items, List<string> order) : ISeasonalRefreshWindowStore
{
    public Dictionary<string, SeasonalRefreshWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalRefreshWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalRefreshWindow item)
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

    public List<SeasonalRefreshWindow> List()
    {
        var outValue = new List<SeasonalRefreshWindow>();
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
public sealed class CachedSeasonalRefreshWindowStore(MemorySeasonalRefreshWindowStore inner, SeasonalRefreshWindowCache cache, int hits, int misses) : ISeasonalRefreshWindowStore
{
    public MemorySeasonalRefreshWindowStore Inner { get; set; } = inner;
    public SeasonalRefreshWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalRefreshWindow? Find(string code)
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

    public void Save(SeasonalRefreshWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalRefreshWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal refresh windows. Empty fields match all.</summary>
public sealed record SeasonalRefreshWindowQuery(string Title, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalRefreshWindow item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalRefreshWindow> Apply(List<SeasonalRefreshWindow> items)
    {
        var matched = Collections.Filter(items, (SeasonalRefreshWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal refresh window store.</summary>
public static class SeasonalRefreshWindowStore
{
    public static ISeasonalRefreshWindowStore NewMemorySeasonalRefreshWindowStore()
    {
        return new MemorySeasonalRefreshWindowStore(
            new Dictionary<string, SeasonalRefreshWindow>(),
            new List<string>());
    }

    public static ISeasonalRefreshWindowStore NewCachedSeasonalRefreshWindowStore(MemorySeasonalRefreshWindowStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalRefreshWindowStore(
            inner,
            SeasonalRefreshWindowCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalRefreshWindow> FindSeasonalRefreshWindows(ISeasonalRefreshWindowStore store, SeasonalRefreshWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
