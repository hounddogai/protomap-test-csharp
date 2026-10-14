namespace Acme.Workforce.Payrollops.Garnishment.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment reservations by code.</summary>
public interface IGarnishmentReservationStore
{
    GarnishmentReservation? Find(string code);
    void Save(GarnishmentReservation item);
    bool Remove(string code);
    List<GarnishmentReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentReservationStore(Dictionary<string, GarnishmentReservation> items, List<string> order) : IGarnishmentReservationStore
{
    public Dictionary<string, GarnishmentReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentReservation item)
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

    public List<GarnishmentReservation> List()
    {
        var outValue = new List<GarnishmentReservation>();
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
public sealed class CachedGarnishmentReservationStore(MemoryGarnishmentReservationStore inner, GarnishmentReservationCache cache, int hits, int misses) : IGarnishmentReservationStore
{
    public MemoryGarnishmentReservationStore Inner { get; set; } = inner;
    public GarnishmentReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentReservation? Find(string code)
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

    public void Save(GarnishmentReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment reservations. Empty fields match all.</summary>
public sealed record GarnishmentReservationQuery(string Label, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentReservation item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GarnishmentReservation> Apply(List<GarnishmentReservation> items)
    {
        var matched = Collections.Filter(items, (GarnishmentReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment reservation store.</summary>
public static class GarnishmentReservationStore
{
    public static IGarnishmentReservationStore NewMemoryGarnishmentReservationStore()
    {
        return new MemoryGarnishmentReservationStore(
            new Dictionary<string, GarnishmentReservation>(),
            new List<string>());
    }

    public static IGarnishmentReservationStore NewCachedGarnishmentReservationStore(MemoryGarnishmentReservationStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentReservationStore(
            inner,
            GarnishmentReservationCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<GarnishmentReservation> FindGarnishmentReservations(IGarnishmentReservationStore store, GarnishmentReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
