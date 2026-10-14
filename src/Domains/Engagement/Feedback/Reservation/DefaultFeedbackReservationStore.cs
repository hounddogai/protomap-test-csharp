namespace Acme.Workforce.Engagement.Feedback.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default feedback reservations by code.</summary>
public interface IDefaultFeedbackReservationStore
{
    DefaultFeedbackReservation? Find(string code);
    void Save(DefaultFeedbackReservation item);
    bool Remove(string code);
    List<DefaultFeedbackReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultFeedbackReservationStore(Dictionary<string, DefaultFeedbackReservation> items, List<string> order) : IDefaultFeedbackReservationStore
{
    public Dictionary<string, DefaultFeedbackReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultFeedbackReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultFeedbackReservation item)
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

    public List<DefaultFeedbackReservation> List()
    {
        var outValue = new List<DefaultFeedbackReservation>();
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
public sealed class CachedDefaultFeedbackReservationStore(MemoryDefaultFeedbackReservationStore inner, DefaultFeedbackReservationCache cache, int hits, int misses) : IDefaultFeedbackReservationStore
{
    public MemoryDefaultFeedbackReservationStore Inner { get; set; } = inner;
    public DefaultFeedbackReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultFeedbackReservation? Find(string code)
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

    public void Save(DefaultFeedbackReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultFeedbackReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default feedback reservations. Empty fields match all.</summary>
public sealed record DefaultFeedbackReservationQuery(string Label, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultFeedbackReservation item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultFeedbackReservation> Apply(List<DefaultFeedbackReservation> items)
    {
        var matched = Collections.Filter(items, (DefaultFeedbackReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default feedback reservation store.</summary>
public static class DefaultFeedbackReservationStore
{
    public static IDefaultFeedbackReservationStore NewMemoryDefaultFeedbackReservationStore()
    {
        return new MemoryDefaultFeedbackReservationStore(
            new Dictionary<string, DefaultFeedbackReservation>(),
            new List<string>());
    }

    public static IDefaultFeedbackReservationStore NewCachedDefaultFeedbackReservationStore(MemoryDefaultFeedbackReservationStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultFeedbackReservationStore(
            inner,
            DefaultFeedbackReservationCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<DefaultFeedbackReservation> FindDefaultFeedbackReservations(IDefaultFeedbackReservationStore store, DefaultFeedbackReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
