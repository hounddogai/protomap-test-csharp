namespace Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal headcount reservations by code.</summary>
public interface ISeasonalHeadcountReservationStore
{
    SeasonalHeadcountReservation? Find(string code);
    void Save(SeasonalHeadcountReservation item);
    bool Remove(string code);
    List<SeasonalHeadcountReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalHeadcountReservationStore(Dictionary<string, SeasonalHeadcountReservation> items, List<string> order) : ISeasonalHeadcountReservationStore
{
    public Dictionary<string, SeasonalHeadcountReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalHeadcountReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalHeadcountReservation item)
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

    public List<SeasonalHeadcountReservation> List()
    {
        var outValue = new List<SeasonalHeadcountReservation>();
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
public sealed class CachedSeasonalHeadcountReservationStore(MemorySeasonalHeadcountReservationStore inner, SeasonalHeadcountReservationCache cache, int hits, int misses) : ISeasonalHeadcountReservationStore
{
    public MemorySeasonalHeadcountReservationStore Inner { get; set; } = inner;
    public SeasonalHeadcountReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalHeadcountReservation? Find(string code)
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

    public void Save(SeasonalHeadcountReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalHeadcountReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal headcount reservations. Empty fields match all.</summary>
public sealed record SeasonalHeadcountReservationQuery(string Department, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalHeadcountReservation item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<SeasonalHeadcountReservation> Apply(List<SeasonalHeadcountReservation> items)
    {
        var matched = Collections.Filter(items, (SeasonalHeadcountReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal headcount reservation store.</summary>
public static class SeasonalHeadcountReservationStore
{
    public static ISeasonalHeadcountReservationStore NewMemorySeasonalHeadcountReservationStore()
    {
        return new MemorySeasonalHeadcountReservationStore(
            new Dictionary<string, SeasonalHeadcountReservation>(),
            new List<string>());
    }

    public static ISeasonalHeadcountReservationStore NewCachedSeasonalHeadcountReservationStore(MemorySeasonalHeadcountReservationStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalHeadcountReservationStore(
            inner,
            SeasonalHeadcountReservationCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalHeadcountReservation> FindSeasonalHeadcountReservations(ISeasonalHeadcountReservationStore store, SeasonalHeadcountReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
