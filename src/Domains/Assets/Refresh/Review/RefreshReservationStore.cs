namespace Acme.Workforce.Assets.Refresh.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh reservations by code.</summary>
public interface IRefreshReservationStore
{
    RefreshReservation? Find(string code);
    void Save(RefreshReservation item);
    bool Remove(string code);
    List<RefreshReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshReservationStore(Dictionary<string, RefreshReservation> items, List<string> order) : IRefreshReservationStore
{
    public Dictionary<string, RefreshReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshReservation item)
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

    public List<RefreshReservation> List()
    {
        var outValue = new List<RefreshReservation>();
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
public sealed class CachedRefreshReservationStore(MemoryRefreshReservationStore inner, RefreshReservationCache cache, int hits, int misses) : IRefreshReservationStore
{
    public MemoryRefreshReservationStore Inner { get; set; } = inner;
    public RefreshReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshReservation? Find(string code)
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

    public void Save(RefreshReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh reservations. Empty fields match all.</summary>
public sealed record RefreshReservationQuery(string Channel, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshReservation item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RefreshReservation> Apply(List<RefreshReservation> items)
    {
        var matched = Collections.Filter(items, (RefreshReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh reservation store.</summary>
public static class RefreshReservationStore
{
    public static IRefreshReservationStore NewMemoryRefreshReservationStore()
    {
        return new MemoryRefreshReservationStore(new Dictionary<string, RefreshReservation>(), new List<string>());
    }

    public static IRefreshReservationStore NewCachedRefreshReservationStore(MemoryRefreshReservationStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshReservationStore(inner, RefreshReservationCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RefreshReservation> FindRefreshReservations(IRefreshReservationStore store, RefreshReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
