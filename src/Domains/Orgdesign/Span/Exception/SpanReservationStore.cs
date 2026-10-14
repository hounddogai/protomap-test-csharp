namespace Acme.Workforce.Orgdesign.Span.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span reservations by code.</summary>
public interface ISpanReservationStore
{
    SpanReservation? Find(string code);
    void Save(SpanReservation item);
    bool Remove(string code);
    List<SpanReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanReservationStore(Dictionary<string, SpanReservation> items, List<string> order) : ISpanReservationStore
{
    public Dictionary<string, SpanReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanReservation item)
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

    public List<SpanReservation> List()
    {
        var outValue = new List<SpanReservation>();
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
public sealed class CachedSpanReservationStore(MemorySpanReservationStore inner, SpanReservationCache cache, int hits, int misses) : ISpanReservationStore
{
    public MemorySpanReservationStore Inner { get; set; } = inner;
    public SpanReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanReservation? Find(string code)
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

    public void Save(SpanReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span reservations. Empty fields match all.</summary>
public sealed record SpanReservationQuery(string Region, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(SpanReservation item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanReservation> Apply(List<SpanReservation> items)
    {
        var matched = Collections.Filter(items, (SpanReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span reservation store.</summary>
public static class SpanReservationStore
{
    public static ISpanReservationStore NewMemorySpanReservationStore()
    {
        return new MemorySpanReservationStore(new Dictionary<string, SpanReservation>(), new List<string>());
    }

    public static ISpanReservationStore NewCachedSpanReservationStore(MemorySpanReservationStore inner, int ttl, IClock clock)
    {
        return new CachedSpanReservationStore(inner, SpanReservationCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SpanReservation> FindSpanReservations(ISpanReservationStore store, SpanReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
