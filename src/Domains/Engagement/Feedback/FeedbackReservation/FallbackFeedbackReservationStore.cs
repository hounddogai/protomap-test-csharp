namespace Acme.Workforce.Engagement.Feedback.FeedbackReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback feedback reservations by code.</summary>
public interface IFallbackFeedbackReservationStore
{
    FallbackFeedbackReservation? Find(string code);
    void Save(FallbackFeedbackReservation item);
    bool Remove(string code);
    List<FallbackFeedbackReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackFeedbackReservationStore(Dictionary<string, FallbackFeedbackReservation> items, List<string> order) : IFallbackFeedbackReservationStore
{
    public Dictionary<string, FallbackFeedbackReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackFeedbackReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackFeedbackReservation item)
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

    public List<FallbackFeedbackReservation> List()
    {
        var outValue = new List<FallbackFeedbackReservation>();
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
public sealed class CachedFallbackFeedbackReservationStore(MemoryFallbackFeedbackReservationStore inner, FallbackFeedbackReservationCache cache, int hits, int misses) : IFallbackFeedbackReservationStore
{
    public MemoryFallbackFeedbackReservationStore Inner { get; set; } = inner;
    public FallbackFeedbackReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackFeedbackReservation? Find(string code)
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

    public void Save(FallbackFeedbackReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackFeedbackReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback feedback reservations. Empty fields match all.</summary>
public sealed record FallbackFeedbackReservationQuery(string Label, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackFeedbackReservation item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackFeedbackReservation> Apply(List<FallbackFeedbackReservation> items)
    {
        var matched = Collections.Filter(items, (FallbackFeedbackReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback feedback reservation store.</summary>
public static class FallbackFeedbackReservationStore
{
    public static IFallbackFeedbackReservationStore NewMemoryFallbackFeedbackReservationStore()
    {
        return new MemoryFallbackFeedbackReservationStore(
            new Dictionary<string, FallbackFeedbackReservation>(),
            new List<string>());
    }

    public static IFallbackFeedbackReservationStore NewCachedFallbackFeedbackReservationStore(MemoryFallbackFeedbackReservationStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackFeedbackReservationStore(
            inner,
            FallbackFeedbackReservationCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<FallbackFeedbackReservation> FindFallbackFeedbackReservations(IFallbackFeedbackReservationStore store, FallbackFeedbackReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
