namespace Acme.Workforce.Workplace.Booking.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive booking exceptions by code.</summary>
public interface IAdaptiveBookingExceptionStore
{
    AdaptiveBookingException? Find(string code);
    void Save(AdaptiveBookingException item);
    bool Remove(string code);
    List<AdaptiveBookingException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveBookingExceptionStore(Dictionary<string, AdaptiveBookingException> items, List<string> order) : IAdaptiveBookingExceptionStore
{
    public Dictionary<string, AdaptiveBookingException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveBookingException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveBookingException item)
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

    public List<AdaptiveBookingException> List()
    {
        var outValue = new List<AdaptiveBookingException>();
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
public sealed class CachedAdaptiveBookingExceptionStore(MemoryAdaptiveBookingExceptionStore inner, AdaptiveBookingExceptionCache cache, int hits, int misses) : IAdaptiveBookingExceptionStore
{
    public MemoryAdaptiveBookingExceptionStore Inner { get; set; } = inner;
    public AdaptiveBookingExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveBookingException? Find(string code)
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

    public void Save(AdaptiveBookingException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveBookingException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive booking exceptions. Empty fields match all.</summary>
public sealed record AdaptiveBookingExceptionQuery(string Label, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveBookingException item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveBookingException> Apply(List<AdaptiveBookingException> items)
    {
        var matched = Collections.Filter(items, (AdaptiveBookingException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive booking exception store.</summary>
public static class AdaptiveBookingExceptionStore
{
    public static IAdaptiveBookingExceptionStore NewMemoryAdaptiveBookingExceptionStore()
    {
        return new MemoryAdaptiveBookingExceptionStore(
            new Dictionary<string, AdaptiveBookingException>(),
            new List<string>());
    }

    public static IAdaptiveBookingExceptionStore NewCachedAdaptiveBookingExceptionStore(MemoryAdaptiveBookingExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveBookingExceptionStore(
            inner,
            AdaptiveBookingExceptionCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveBookingException> FindAdaptiveBookingExceptions(IAdaptiveBookingExceptionStore store, AdaptiveBookingExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
