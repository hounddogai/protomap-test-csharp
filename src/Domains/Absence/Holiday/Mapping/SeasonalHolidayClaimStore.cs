namespace Acme.Workforce.Absence.Holiday.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal holiday claims by code.</summary>
public interface ISeasonalHolidayClaimStore
{
    SeasonalHolidayClaim? Find(string code);
    void Save(SeasonalHolidayClaim item);
    bool Remove(string code);
    List<SeasonalHolidayClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalHolidayClaimStore(Dictionary<string, SeasonalHolidayClaim> items, List<string> order) : ISeasonalHolidayClaimStore
{
    public Dictionary<string, SeasonalHolidayClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalHolidayClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalHolidayClaim item)
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

    public List<SeasonalHolidayClaim> List()
    {
        var outValue = new List<SeasonalHolidayClaim>();
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
public sealed class CachedSeasonalHolidayClaimStore(MemorySeasonalHolidayClaimStore inner, SeasonalHolidayClaimCache cache, int hits, int misses) : ISeasonalHolidayClaimStore
{
    public MemorySeasonalHolidayClaimStore Inner { get; set; } = inner;
    public SeasonalHolidayClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalHolidayClaim? Find(string code)
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

    public void Save(SeasonalHolidayClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalHolidayClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal holiday claims. Empty fields match all.</summary>
public sealed record SeasonalHolidayClaimQuery(string Segment, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalHolidayClaim item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalHolidayClaim> Apply(List<SeasonalHolidayClaim> items)
    {
        var matched = Collections.Filter(items, (SeasonalHolidayClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal holiday claim store.</summary>
public static class SeasonalHolidayClaimStore
{
    public static ISeasonalHolidayClaimStore NewMemorySeasonalHolidayClaimStore()
    {
        return new MemorySeasonalHolidayClaimStore(
            new Dictionary<string, SeasonalHolidayClaim>(),
            new List<string>());
    }

    public static ISeasonalHolidayClaimStore NewCachedSeasonalHolidayClaimStore(MemorySeasonalHolidayClaimStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalHolidayClaimStore(inner, SeasonalHolidayClaimCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SeasonalHolidayClaim> FindSeasonalHolidayClaims(ISeasonalHolidayClaimStore store, SeasonalHolidayClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
