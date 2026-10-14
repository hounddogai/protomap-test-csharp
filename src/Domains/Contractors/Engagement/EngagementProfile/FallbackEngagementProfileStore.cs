namespace Acme.Workforce.Contractors.Engagement.EngagementProfile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback engagement profiles by code.</summary>
public interface IFallbackEngagementProfileStore
{
    FallbackEngagementProfile? Find(string code);
    void Save(FallbackEngagementProfile item);
    bool Remove(string code);
    List<FallbackEngagementProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackEngagementProfileStore(Dictionary<string, FallbackEngagementProfile> items, List<string> order) : IFallbackEngagementProfileStore
{
    public Dictionary<string, FallbackEngagementProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackEngagementProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackEngagementProfile item)
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

    public List<FallbackEngagementProfile> List()
    {
        var outValue = new List<FallbackEngagementProfile>();
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
public sealed class CachedFallbackEngagementProfileStore(MemoryFallbackEngagementProfileStore inner, FallbackEngagementProfileCache cache, int hits, int misses) : IFallbackEngagementProfileStore
{
    public MemoryFallbackEngagementProfileStore Inner { get; set; } = inner;
    public FallbackEngagementProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackEngagementProfile? Find(string code)
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

    public void Save(FallbackEngagementProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackEngagementProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback engagement profiles. Empty fields match all.</summary>
public sealed record FallbackEngagementProfileQuery(string Name, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackEngagementProfile item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackEngagementProfile> Apply(List<FallbackEngagementProfile> items)
    {
        var matched = Collections.Filter(items, (FallbackEngagementProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback engagement profile store.</summary>
public static class FallbackEngagementProfileStore
{
    public static IFallbackEngagementProfileStore NewMemoryFallbackEngagementProfileStore()
    {
        return new MemoryFallbackEngagementProfileStore(
            new Dictionary<string, FallbackEngagementProfile>(),
            new List<string>());
    }

    public static IFallbackEngagementProfileStore NewCachedFallbackEngagementProfileStore(MemoryFallbackEngagementProfileStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackEngagementProfileStore(
            inner,
            FallbackEngagementProfileCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<FallbackEngagementProfile> FindFallbackEngagementProfiles(IFallbackEngagementProfileStore store, FallbackEngagementProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
