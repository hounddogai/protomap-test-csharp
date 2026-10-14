namespace Acme.Workforce.Talent.Nomination.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy nomination reservations by code.</summary>
public interface ILegacyNominationReservationStore
{
    LegacyNominationReservation? Find(string code);
    void Save(LegacyNominationReservation item);
    bool Remove(string code);
    List<LegacyNominationReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyNominationReservationStore(Dictionary<string, LegacyNominationReservation> items, List<string> order) : ILegacyNominationReservationStore
{
    public Dictionary<string, LegacyNominationReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyNominationReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyNominationReservation item)
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

    public List<LegacyNominationReservation> List()
    {
        var outValue = new List<LegacyNominationReservation>();
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
public sealed class CachedLegacyNominationReservationStore(MemoryLegacyNominationReservationStore inner, LegacyNominationReservationCache cache, int hits, int misses) : ILegacyNominationReservationStore
{
    public MemoryLegacyNominationReservationStore Inner { get; set; } = inner;
    public LegacyNominationReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyNominationReservation? Find(string code)
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

    public void Save(LegacyNominationReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyNominationReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy nomination reservations. Empty fields match all.</summary>
public sealed record LegacyNominationReservationQuery(string Label, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyNominationReservation item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<LegacyNominationReservation> Apply(List<LegacyNominationReservation> items)
    {
        var matched = Collections.Filter(items, (LegacyNominationReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy nomination reservation store.</summary>
public static class LegacyNominationReservationStore
{
    public static ILegacyNominationReservationStore NewMemoryLegacyNominationReservationStore()
    {
        return new MemoryLegacyNominationReservationStore(
            new Dictionary<string, LegacyNominationReservation>(),
            new List<string>());
    }

    public static ILegacyNominationReservationStore NewCachedLegacyNominationReservationStore(MemoryLegacyNominationReservationStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyNominationReservationStore(
            inner,
            LegacyNominationReservationCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LegacyNominationReservation> FindLegacyNominationReservations(ILegacyNominationReservationStore store, LegacyNominationReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
