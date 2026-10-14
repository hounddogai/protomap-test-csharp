namespace Acme.Workforce.Orgdesign.Headcount.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal headcount windows by code.</summary>
public interface ISeasonalHeadcountWindowStore
{
    SeasonalHeadcountWindow? Find(string code);
    void Save(SeasonalHeadcountWindow item);
    bool Remove(string code);
    List<SeasonalHeadcountWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalHeadcountWindowStore(Dictionary<string, SeasonalHeadcountWindow> items, List<string> order) : ISeasonalHeadcountWindowStore
{
    public Dictionary<string, SeasonalHeadcountWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalHeadcountWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalHeadcountWindow item)
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

    public List<SeasonalHeadcountWindow> List()
    {
        var outValue = new List<SeasonalHeadcountWindow>();
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
public sealed class CachedSeasonalHeadcountWindowStore(MemorySeasonalHeadcountWindowStore inner, SeasonalHeadcountWindowCache cache, int hits, int misses) : ISeasonalHeadcountWindowStore
{
    public MemorySeasonalHeadcountWindowStore Inner { get; set; } = inner;
    public SeasonalHeadcountWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalHeadcountWindow? Find(string code)
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

    public void Save(SeasonalHeadcountWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalHeadcountWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal headcount windows. Empty fields match all.</summary>
public sealed record SeasonalHeadcountWindowQuery(string Region, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalHeadcountWindow item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalHeadcountWindow> Apply(List<SeasonalHeadcountWindow> items)
    {
        var matched = Collections.Filter(items, (SeasonalHeadcountWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal headcount window store.</summary>
public static class SeasonalHeadcountWindowStore
{
    public static ISeasonalHeadcountWindowStore NewMemorySeasonalHeadcountWindowStore()
    {
        return new MemorySeasonalHeadcountWindowStore(
            new Dictionary<string, SeasonalHeadcountWindow>(),
            new List<string>());
    }

    public static ISeasonalHeadcountWindowStore NewCachedSeasonalHeadcountWindowStore(MemorySeasonalHeadcountWindowStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalHeadcountWindowStore(
            inner,
            SeasonalHeadcountWindowCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalHeadcountWindow> FindSeasonalHeadcountWindows(ISeasonalHeadcountWindowStore store, SeasonalHeadcountWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
