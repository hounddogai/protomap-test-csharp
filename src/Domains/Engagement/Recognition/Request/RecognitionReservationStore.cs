namespace Acme.Workforce.Engagement.Recognition.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition reservations by code.</summary>
public interface IRecognitionReservationStore
{
    RecognitionReservation? Find(string code);
    void Save(RecognitionReservation item);
    bool Remove(string code);
    List<RecognitionReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionReservationStore(Dictionary<string, RecognitionReservation> items, List<string> order) : IRecognitionReservationStore
{
    public Dictionary<string, RecognitionReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionReservation item)
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

    public List<RecognitionReservation> List()
    {
        var outValue = new List<RecognitionReservation>();
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
public sealed class CachedRecognitionReservationStore(MemoryRecognitionReservationStore inner, RecognitionReservationCache cache, int hits, int misses) : IRecognitionReservationStore
{
    public MemoryRecognitionReservationStore Inner { get; set; } = inner;
    public RecognitionReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionReservation? Find(string code)
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

    public void Save(RecognitionReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition reservations. Empty fields match all.</summary>
public sealed record RecognitionReservationQuery(string Category, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionReservation item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RecognitionReservation> Apply(List<RecognitionReservation> items)
    {
        var matched = Collections.Filter(items, (RecognitionReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition reservation store.</summary>
public static class RecognitionReservationStore
{
    public static IRecognitionReservationStore NewMemoryRecognitionReservationStore()
    {
        return new MemoryRecognitionReservationStore(
            new Dictionary<string, RecognitionReservation>(),
            new List<string>());
    }

    public static IRecognitionReservationStore NewCachedRecognitionReservationStore(MemoryRecognitionReservationStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionReservationStore(
            inner,
            RecognitionReservationCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<RecognitionReservation> FindRecognitionReservations(IRecognitionReservationStore store, RecognitionReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
