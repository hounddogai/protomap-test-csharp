namespace Acme.Workforce.Absence.Holiday.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal holiday exceptions by code.</summary>
public interface ISeasonalHolidayExceptionStore
{
    SeasonalHolidayException? Find(string code);
    void Save(SeasonalHolidayException item);
    bool Remove(string code);
    List<SeasonalHolidayException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalHolidayExceptionStore(Dictionary<string, SeasonalHolidayException> items, List<string> order) : ISeasonalHolidayExceptionStore
{
    public Dictionary<string, SeasonalHolidayException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalHolidayException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalHolidayException item)
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

    public List<SeasonalHolidayException> List()
    {
        var outValue = new List<SeasonalHolidayException>();
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
public sealed class CachedSeasonalHolidayExceptionStore(MemorySeasonalHolidayExceptionStore inner, SeasonalHolidayExceptionCache cache, int hits, int misses) : ISeasonalHolidayExceptionStore
{
    public MemorySeasonalHolidayExceptionStore Inner { get; set; } = inner;
    public SeasonalHolidayExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalHolidayException? Find(string code)
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

    public void Save(SeasonalHolidayException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalHolidayException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal holiday exceptions. Empty fields match all.</summary>
public sealed record SeasonalHolidayExceptionQuery(string Locale, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalHolidayException item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalHolidayException> Apply(List<SeasonalHolidayException> items)
    {
        var matched = Collections.Filter(items, (SeasonalHolidayException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal holiday exception store.</summary>
public static class SeasonalHolidayExceptionStore
{
    public static ISeasonalHolidayExceptionStore NewMemorySeasonalHolidayExceptionStore()
    {
        return new MemorySeasonalHolidayExceptionStore(
            new Dictionary<string, SeasonalHolidayException>(),
            new List<string>());
    }

    public static ISeasonalHolidayExceptionStore NewCachedSeasonalHolidayExceptionStore(MemorySeasonalHolidayExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalHolidayExceptionStore(
            inner,
            SeasonalHolidayExceptionCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalHolidayException> FindSeasonalHolidayExceptions(ISeasonalHolidayExceptionStore store, SeasonalHolidayExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
