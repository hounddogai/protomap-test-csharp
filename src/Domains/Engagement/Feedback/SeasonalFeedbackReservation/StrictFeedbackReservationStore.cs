namespace Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict feedback reservations by code.</summary>
public interface IStrictFeedbackReservationStore
{
    StrictFeedbackReservation? Find(string code);
    void Save(StrictFeedbackReservation item);
    bool Remove(string code);
    List<StrictFeedbackReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictFeedbackReservationStore(Dictionary<string, StrictFeedbackReservation> items, List<string> order) : IStrictFeedbackReservationStore
{
    public Dictionary<string, StrictFeedbackReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictFeedbackReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictFeedbackReservation item)
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

    public List<StrictFeedbackReservation> List()
    {
        var outValue = new List<StrictFeedbackReservation>();
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
public sealed class CachedStrictFeedbackReservationStore(MemoryStrictFeedbackReservationStore inner, StrictFeedbackReservationCache cache, int hits, int misses) : IStrictFeedbackReservationStore
{
    public MemoryStrictFeedbackReservationStore Inner { get; set; } = inner;
    public StrictFeedbackReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictFeedbackReservation? Find(string code)
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

    public void Save(StrictFeedbackReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictFeedbackReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict feedback reservations. Empty fields match all.</summary>
public sealed record StrictFeedbackReservationQuery(string Label, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(StrictFeedbackReservation item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictFeedbackReservation> Apply(List<StrictFeedbackReservation> items)
    {
        var matched = Collections.Filter(items, (StrictFeedbackReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict feedback reservation store.</summary>
public static class StrictFeedbackReservationStore
{
    public static IStrictFeedbackReservationStore NewMemoryStrictFeedbackReservationStore()
    {
        return new MemoryStrictFeedbackReservationStore(
            new Dictionary<string, StrictFeedbackReservation>(),
            new List<string>());
    }

    public static IStrictFeedbackReservationStore NewCachedStrictFeedbackReservationStore(MemoryStrictFeedbackReservationStore inner, int ttl, IClock clock)
    {
        return new CachedStrictFeedbackReservationStore(
            inner,
            StrictFeedbackReservationCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StrictFeedbackReservation> FindStrictFeedbackReservations(IStrictFeedbackReservationStore store, StrictFeedbackReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
