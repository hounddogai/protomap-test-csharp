namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling sentiment reservations by code.</summary>
public interface IRollingSentimentReservationStore
{
    RollingSentimentReservation? Find(string code);
    void Save(RollingSentimentReservation item);
    bool Remove(string code);
    List<RollingSentimentReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingSentimentReservationStore(Dictionary<string, RollingSentimentReservation> items, List<string> order) : IRollingSentimentReservationStore
{
    public Dictionary<string, RollingSentimentReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingSentimentReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingSentimentReservation item)
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

    public List<RollingSentimentReservation> List()
    {
        var outValue = new List<RollingSentimentReservation>();
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
public sealed class CachedRollingSentimentReservationStore(MemoryRollingSentimentReservationStore inner, RollingSentimentReservationCache cache, int hits, int misses) : IRollingSentimentReservationStore
{
    public MemoryRollingSentimentReservationStore Inner { get; set; } = inner;
    public RollingSentimentReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingSentimentReservation? Find(string code)
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

    public void Save(RollingSentimentReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingSentimentReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling sentiment reservations. Empty fields match all.</summary>
public sealed record RollingSentimentReservationQuery(string Name, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(RollingSentimentReservation item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingSentimentReservation> Apply(List<RollingSentimentReservation> items)
    {
        var matched = Collections.Filter(items, (RollingSentimentReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling sentiment reservation store.</summary>
public static class RollingSentimentReservationStore
{
    public static IRollingSentimentReservationStore NewMemoryRollingSentimentReservationStore()
    {
        return new MemoryRollingSentimentReservationStore(
            new Dictionary<string, RollingSentimentReservation>(),
            new List<string>());
    }

    public static IRollingSentimentReservationStore NewCachedRollingSentimentReservationStore(MemoryRollingSentimentReservationStore inner, int ttl, IClock clock)
    {
        return new CachedRollingSentimentReservationStore(
            inner,
            RollingSentimentReservationCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RollingSentimentReservation> FindRollingSentimentReservations(IRollingSentimentReservationStore store, RollingSentimentReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
