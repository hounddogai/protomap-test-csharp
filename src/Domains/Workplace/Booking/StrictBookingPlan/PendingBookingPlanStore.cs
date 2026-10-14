namespace Acme.Workforce.Workplace.Booking.StrictBookingPlan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending booking plans by code.</summary>
public interface IPendingBookingPlanStore
{
    PendingBookingPlan? Find(string code);
    void Save(PendingBookingPlan item);
    bool Remove(string code);
    List<PendingBookingPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingBookingPlanStore(Dictionary<string, PendingBookingPlan> items, List<string> order) : IPendingBookingPlanStore
{
    public Dictionary<string, PendingBookingPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingBookingPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingBookingPlan item)
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

    public List<PendingBookingPlan> List()
    {
        var outValue = new List<PendingBookingPlan>();
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
public sealed class CachedPendingBookingPlanStore(MemoryPendingBookingPlanStore inner, PendingBookingPlanCache cache, int hits, int misses) : IPendingBookingPlanStore
{
    public MemoryPendingBookingPlanStore Inner { get; set; } = inner;
    public PendingBookingPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingBookingPlan? Find(string code)
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

    public void Save(PendingBookingPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingBookingPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending booking plans. Empty fields match all.</summary>
public sealed record PendingBookingPlanQuery(string Label, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(PendingBookingPlan item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<PendingBookingPlan> Apply(List<PendingBookingPlan> items)
    {
        var matched = Collections.Filter(items, (PendingBookingPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending booking plan store.</summary>
public static class PendingBookingPlanStore
{
    public static IPendingBookingPlanStore NewMemoryPendingBookingPlanStore()
    {
        return new MemoryPendingBookingPlanStore(new Dictionary<string, PendingBookingPlan>(), new List<string>());
    }

    public static IPendingBookingPlanStore NewCachedPendingBookingPlanStore(MemoryPendingBookingPlanStore inner, int ttl, IClock clock)
    {
        return new CachedPendingBookingPlanStore(inner, PendingBookingPlanCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PendingBookingPlan> FindPendingBookingPlans(IPendingBookingPlanStore store, PendingBookingPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
