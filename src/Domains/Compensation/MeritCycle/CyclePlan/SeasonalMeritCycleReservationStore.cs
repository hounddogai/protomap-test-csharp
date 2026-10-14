namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal merit cycle reservations by code.</summary>
public interface ISeasonalMeritCycleReservationStore
{
    SeasonalMeritCycleReservation? Find(string code);
    void Save(SeasonalMeritCycleReservation item);
    bool Remove(string code);
    List<SeasonalMeritCycleReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalMeritCycleReservationStore(Dictionary<string, SeasonalMeritCycleReservation> items, List<string> order) : ISeasonalMeritCycleReservationStore
{
    public Dictionary<string, SeasonalMeritCycleReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalMeritCycleReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalMeritCycleReservation item)
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

    public List<SeasonalMeritCycleReservation> List()
    {
        var outValue = new List<SeasonalMeritCycleReservation>();
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
public sealed class CachedSeasonalMeritCycleReservationStore(MemorySeasonalMeritCycleReservationStore inner, SeasonalMeritCycleReservationCache cache, int hits, int misses) : ISeasonalMeritCycleReservationStore
{
    public MemorySeasonalMeritCycleReservationStore Inner { get; set; } = inner;
    public SeasonalMeritCycleReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalMeritCycleReservation? Find(string code)
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

    public void Save(SeasonalMeritCycleReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalMeritCycleReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal merit cycle reservations. Empty fields match all.</summary>
public sealed record SeasonalMeritCycleReservationQuery(string Title, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalMeritCycleReservation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalMeritCycleReservation> Apply(List<SeasonalMeritCycleReservation> items)
    {
        var matched = Collections.Filter(items, (SeasonalMeritCycleReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal merit cycle reservation store.</summary>
public static class SeasonalMeritCycleReservationStore
{
    public static ISeasonalMeritCycleReservationStore NewMemorySeasonalMeritCycleReservationStore()
    {
        return new MemorySeasonalMeritCycleReservationStore(
            new Dictionary<string, SeasonalMeritCycleReservation>(),
            new List<string>());
    }

    public static ISeasonalMeritCycleReservationStore NewCachedSeasonalMeritCycleReservationStore(MemorySeasonalMeritCycleReservationStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalMeritCycleReservationStore(
            inner,
            SeasonalMeritCycleReservationCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalMeritCycleReservation> FindSeasonalMeritCycleReservations(ISeasonalMeritCycleReservationStore store, SeasonalMeritCycleReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
