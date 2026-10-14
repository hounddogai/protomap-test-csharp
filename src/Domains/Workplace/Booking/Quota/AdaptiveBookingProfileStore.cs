namespace Acme.Workforce.Workplace.Booking.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive booking profiles by code.</summary>
public interface IAdaptiveBookingProfileStore
{
    AdaptiveBookingProfile? Find(string code);
    void Save(AdaptiveBookingProfile item);
    bool Remove(string code);
    List<AdaptiveBookingProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveBookingProfileStore(Dictionary<string, AdaptiveBookingProfile> items, List<string> order) : IAdaptiveBookingProfileStore
{
    public Dictionary<string, AdaptiveBookingProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveBookingProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveBookingProfile item)
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

    public List<AdaptiveBookingProfile> List()
    {
        var outValue = new List<AdaptiveBookingProfile>();
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
public sealed class CachedAdaptiveBookingProfileStore(MemoryAdaptiveBookingProfileStore inner, AdaptiveBookingProfileCache cache, int hits, int misses) : IAdaptiveBookingProfileStore
{
    public MemoryAdaptiveBookingProfileStore Inner { get; set; } = inner;
    public AdaptiveBookingProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveBookingProfile? Find(string code)
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

    public void Save(AdaptiveBookingProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveBookingProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive booking profiles. Empty fields match all.</summary>
public sealed record AdaptiveBookingProfileQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveBookingProfile item)
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

    public List<AdaptiveBookingProfile> Apply(List<AdaptiveBookingProfile> items)
    {
        var matched = Collections.Filter(items, (AdaptiveBookingProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive booking profile store.</summary>
public static class AdaptiveBookingProfileStore
{
    public static IAdaptiveBookingProfileStore NewMemoryAdaptiveBookingProfileStore()
    {
        return new MemoryAdaptiveBookingProfileStore(
            new Dictionary<string, AdaptiveBookingProfile>(),
            new List<string>());
    }

    public static IAdaptiveBookingProfileStore NewCachedAdaptiveBookingProfileStore(MemoryAdaptiveBookingProfileStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveBookingProfileStore(
            inner,
            AdaptiveBookingProfileCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveBookingProfile> FindAdaptiveBookingProfiles(IAdaptiveBookingProfileStore store, AdaptiveBookingProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
