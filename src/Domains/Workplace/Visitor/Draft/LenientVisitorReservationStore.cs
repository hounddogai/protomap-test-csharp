namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient visitor reservations by code.</summary>
public interface ILenientVisitorReservationStore
{
    LenientVisitorReservation? Find(string code);
    void Save(LenientVisitorReservation item);
    bool Remove(string code);
    List<LenientVisitorReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientVisitorReservationStore(Dictionary<string, LenientVisitorReservation> items, List<string> order) : ILenientVisitorReservationStore
{
    public Dictionary<string, LenientVisitorReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientVisitorReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientVisitorReservation item)
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

    public List<LenientVisitorReservation> List()
    {
        var outValue = new List<LenientVisitorReservation>();
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
public sealed class CachedLenientVisitorReservationStore(MemoryLenientVisitorReservationStore inner, LenientVisitorReservationCache cache, int hits, int misses) : ILenientVisitorReservationStore
{
    public MemoryLenientVisitorReservationStore Inner { get; set; } = inner;
    public LenientVisitorReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientVisitorReservation? Find(string code)
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

    public void Save(LenientVisitorReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientVisitorReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient visitor reservations. Empty fields match all.</summary>
public sealed record LenientVisitorReservationQuery(string Name, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(LenientVisitorReservation item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientVisitorReservation> Apply(List<LenientVisitorReservation> items)
    {
        var matched = Collections.Filter(items, (LenientVisitorReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient visitor reservation store.</summary>
public static class LenientVisitorReservationStore
{
    public static ILenientVisitorReservationStore NewMemoryLenientVisitorReservationStore()
    {
        return new MemoryLenientVisitorReservationStore(
            new Dictionary<string, LenientVisitorReservation>(),
            new List<string>());
    }

    public static ILenientVisitorReservationStore NewCachedLenientVisitorReservationStore(MemoryLenientVisitorReservationStore inner, int ttl, IClock clock)
    {
        return new CachedLenientVisitorReservationStore(
            inner,
            LenientVisitorReservationCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LenientVisitorReservation> FindLenientVisitorReservations(ILenientVisitorReservationStore store, LenientVisitorReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
