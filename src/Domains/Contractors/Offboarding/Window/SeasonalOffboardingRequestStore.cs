namespace Acme.Workforce.Contractors.Offboarding.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal offboarding requests by code.</summary>
public interface ISeasonalOffboardingRequestStore
{
    SeasonalOffboardingRequest? Find(string code);
    void Save(SeasonalOffboardingRequest item);
    bool Remove(string code);
    List<SeasonalOffboardingRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalOffboardingRequestStore(Dictionary<string, SeasonalOffboardingRequest> items, List<string> order) : ISeasonalOffboardingRequestStore
{
    public Dictionary<string, SeasonalOffboardingRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalOffboardingRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalOffboardingRequest item)
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

    public List<SeasonalOffboardingRequest> List()
    {
        var outValue = new List<SeasonalOffboardingRequest>();
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
public sealed class CachedSeasonalOffboardingRequestStore(MemorySeasonalOffboardingRequestStore inner, SeasonalOffboardingRequestCache cache, int hits, int misses) : ISeasonalOffboardingRequestStore
{
    public MemorySeasonalOffboardingRequestStore Inner { get; set; } = inner;
    public SeasonalOffboardingRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalOffboardingRequest? Find(string code)
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

    public void Save(SeasonalOffboardingRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalOffboardingRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal offboarding requests. Empty fields match all.</summary>
public sealed record SeasonalOffboardingRequestQuery(string Label, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalOffboardingRequest item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalOffboardingRequest> Apply(List<SeasonalOffboardingRequest> items)
    {
        var matched = Collections.Filter(items, (SeasonalOffboardingRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal offboarding request store.</summary>
public static class SeasonalOffboardingRequestStore
{
    public static ISeasonalOffboardingRequestStore NewMemorySeasonalOffboardingRequestStore()
    {
        return new MemorySeasonalOffboardingRequestStore(
            new Dictionary<string, SeasonalOffboardingRequest>(),
            new List<string>());
    }

    public static ISeasonalOffboardingRequestStore NewCachedSeasonalOffboardingRequestStore(MemorySeasonalOffboardingRequestStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalOffboardingRequestStore(
            inner,
            SeasonalOffboardingRequestCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalOffboardingRequest> FindSeasonalOffboardingRequests(ISeasonalOffboardingRequestStore store, SeasonalOffboardingRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
