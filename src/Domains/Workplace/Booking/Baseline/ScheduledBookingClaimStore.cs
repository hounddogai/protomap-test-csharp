namespace Acme.Workforce.Workplace.Booking.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled booking claims by code.</summary>
public interface IScheduledBookingClaimStore
{
    ScheduledBookingClaim? Find(string code);
    void Save(ScheduledBookingClaim item);
    bool Remove(string code);
    List<ScheduledBookingClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledBookingClaimStore(Dictionary<string, ScheduledBookingClaim> items, List<string> order) : IScheduledBookingClaimStore
{
    public Dictionary<string, ScheduledBookingClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledBookingClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledBookingClaim item)
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

    public List<ScheduledBookingClaim> List()
    {
        var outValue = new List<ScheduledBookingClaim>();
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
public sealed class CachedScheduledBookingClaimStore(MemoryScheduledBookingClaimStore inner, ScheduledBookingClaimCache cache, int hits, int misses) : IScheduledBookingClaimStore
{
    public MemoryScheduledBookingClaimStore Inner { get; set; } = inner;
    public ScheduledBookingClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledBookingClaim? Find(string code)
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

    public void Save(ScheduledBookingClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledBookingClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled booking claims. Empty fields match all.</summary>
public sealed record ScheduledBookingClaimQuery(string Category, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledBookingClaim item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledBookingClaim> Apply(List<ScheduledBookingClaim> items)
    {
        var matched = Collections.Filter(items, (ScheduledBookingClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled booking claim store.</summary>
public static class ScheduledBookingClaimStore
{
    public static IScheduledBookingClaimStore NewMemoryScheduledBookingClaimStore()
    {
        return new MemoryScheduledBookingClaimStore(
            new Dictionary<string, ScheduledBookingClaim>(),
            new List<string>());
    }

    public static IScheduledBookingClaimStore NewCachedScheduledBookingClaimStore(MemoryScheduledBookingClaimStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledBookingClaimStore(
            inner,
            ScheduledBookingClaimCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledBookingClaim> FindScheduledBookingClaims(IScheduledBookingClaimStore store, ScheduledBookingClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
