namespace Acme.Workforce.Workplace.Booking.BookingPlan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient booking plans by code.</summary>
public interface ILenientBookingPlanStore
{
    LenientBookingPlan? Find(string code);
    void Save(LenientBookingPlan item);
    bool Remove(string code);
    List<LenientBookingPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientBookingPlanStore(Dictionary<string, LenientBookingPlan> items, List<string> order) : ILenientBookingPlanStore
{
    public Dictionary<string, LenientBookingPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientBookingPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientBookingPlan item)
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

    public List<LenientBookingPlan> List()
    {
        var outValue = new List<LenientBookingPlan>();
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
public sealed class CachedLenientBookingPlanStore(MemoryLenientBookingPlanStore inner, LenientBookingPlanCache cache, int hits, int misses) : ILenientBookingPlanStore
{
    public MemoryLenientBookingPlanStore Inner { get; set; } = inner;
    public LenientBookingPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientBookingPlan? Find(string code)
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

    public void Save(LenientBookingPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientBookingPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient booking plans. Empty fields match all.</summary>
public sealed record LenientBookingPlanQuery(string Segment, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(LenientBookingPlan item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientBookingPlan> Apply(List<LenientBookingPlan> items)
    {
        var matched = Collections.Filter(items, (LenientBookingPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient booking plan store.</summary>
public static class LenientBookingPlanStore
{
    public static ILenientBookingPlanStore NewMemoryLenientBookingPlanStore()
    {
        return new MemoryLenientBookingPlanStore(new Dictionary<string, LenientBookingPlan>(), new List<string>());
    }

    public static ILenientBookingPlanStore NewCachedLenientBookingPlanStore(MemoryLenientBookingPlanStore inner, int ttl, IClock clock)
    {
        return new CachedLenientBookingPlanStore(inner, LenientBookingPlanCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LenientBookingPlan> FindLenientBookingPlans(ILenientBookingPlanStore store, LenientBookingPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
