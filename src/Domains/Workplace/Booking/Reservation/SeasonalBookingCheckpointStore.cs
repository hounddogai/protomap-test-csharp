namespace Acme.Workforce.Workplace.Booking.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal booking checkpoints by code.</summary>
public interface ISeasonalBookingCheckpointStore
{
    SeasonalBookingCheckpoint? Find(string code);
    void Save(SeasonalBookingCheckpoint item);
    bool Remove(string code);
    List<SeasonalBookingCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalBookingCheckpointStore(Dictionary<string, SeasonalBookingCheckpoint> items, List<string> order) : ISeasonalBookingCheckpointStore
{
    public Dictionary<string, SeasonalBookingCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalBookingCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalBookingCheckpoint item)
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

    public List<SeasonalBookingCheckpoint> List()
    {
        var outValue = new List<SeasonalBookingCheckpoint>();
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
public sealed class CachedSeasonalBookingCheckpointStore(MemorySeasonalBookingCheckpointStore inner, SeasonalBookingCheckpointCache cache, int hits, int misses) : ISeasonalBookingCheckpointStore
{
    public MemorySeasonalBookingCheckpointStore Inner { get; set; } = inner;
    public SeasonalBookingCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalBookingCheckpoint? Find(string code)
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

    public void Save(SeasonalBookingCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalBookingCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal booking checkpoints. Empty fields match all.</summary>
public sealed record SeasonalBookingCheckpointQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalBookingCheckpoint item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalBookingCheckpoint> Apply(List<SeasonalBookingCheckpoint> items)
    {
        var matched = Collections.Filter(items, (SeasonalBookingCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal booking checkpoint store.</summary>
public static class SeasonalBookingCheckpointStore
{
    public static ISeasonalBookingCheckpointStore NewMemorySeasonalBookingCheckpointStore()
    {
        return new MemorySeasonalBookingCheckpointStore(
            new Dictionary<string, SeasonalBookingCheckpoint>(),
            new List<string>());
    }

    public static ISeasonalBookingCheckpointStore NewCachedSeasonalBookingCheckpointStore(MemorySeasonalBookingCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalBookingCheckpointStore(
            inner,
            SeasonalBookingCheckpointCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalBookingCheckpoint> FindSeasonalBookingCheckpoints(ISeasonalBookingCheckpointStore store, SeasonalBookingCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
