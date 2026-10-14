namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps license seat reservations by code.</summary>
public interface ILicenseSeatReservationStore
{
    LicenseSeatReservation? Find(string code);
    void Save(LicenseSeatReservation item);
    bool Remove(string code);
    List<LicenseSeatReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLicenseSeatReservationStore(Dictionary<string, LicenseSeatReservation> items, List<string> order) : ILicenseSeatReservationStore
{
    public Dictionary<string, LicenseSeatReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LicenseSeatReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LicenseSeatReservation item)
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

    public List<LicenseSeatReservation> List()
    {
        var outValue = new List<LicenseSeatReservation>();
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
public sealed class CachedLicenseSeatReservationStore(MemoryLicenseSeatReservationStore inner, LicenseSeatReservationCache cache, int hits, int misses) : ILicenseSeatReservationStore
{
    public MemoryLicenseSeatReservationStore Inner { get; set; } = inner;
    public LicenseSeatReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LicenseSeatReservation? Find(string code)
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

    public void Save(LicenseSeatReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LicenseSeatReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects license seat reservations. Empty fields match all.</summary>
public sealed record LicenseSeatReservationQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(LicenseSeatReservation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LicenseSeatReservation> Apply(List<LicenseSeatReservation> items)
    {
        var matched = Collections.Filter(items, (LicenseSeatReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of license seat reservation store.</summary>
public static class LicenseSeatReservationStore
{
    public static ILicenseSeatReservationStore NewMemoryLicenseSeatReservationStore()
    {
        return new MemoryLicenseSeatReservationStore(
            new Dictionary<string, LicenseSeatReservation>(),
            new List<string>());
    }

    public static ILicenseSeatReservationStore NewCachedLicenseSeatReservationStore(MemoryLicenseSeatReservationStore inner, int ttl, IClock clock)
    {
        return new CachedLicenseSeatReservationStore(
            inner,
            LicenseSeatReservationCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LicenseSeatReservation> FindLicenseSeatReservations(ILicenseSeatReservationStore store, LicenseSeatReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
