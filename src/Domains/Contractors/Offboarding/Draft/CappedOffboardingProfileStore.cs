namespace Acme.Workforce.Contractors.Offboarding.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped offboarding profiles by code.</summary>
public interface ICappedOffboardingProfileStore
{
    CappedOffboardingProfile? Find(string code);
    void Save(CappedOffboardingProfile item);
    bool Remove(string code);
    List<CappedOffboardingProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedOffboardingProfileStore(Dictionary<string, CappedOffboardingProfile> items, List<string> order) : ICappedOffboardingProfileStore
{
    public Dictionary<string, CappedOffboardingProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedOffboardingProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedOffboardingProfile item)
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

    public List<CappedOffboardingProfile> List()
    {
        var outValue = new List<CappedOffboardingProfile>();
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
public sealed class CachedCappedOffboardingProfileStore(MemoryCappedOffboardingProfileStore inner, CappedOffboardingProfileCache cache, int hits, int misses) : ICappedOffboardingProfileStore
{
    public MemoryCappedOffboardingProfileStore Inner { get; set; } = inner;
    public CappedOffboardingProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedOffboardingProfile? Find(string code)
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

    public void Save(CappedOffboardingProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedOffboardingProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped offboarding profiles. Empty fields match all.</summary>
public sealed record CappedOffboardingProfileQuery(string Department, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(CappedOffboardingProfile item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<CappedOffboardingProfile> Apply(List<CappedOffboardingProfile> items)
    {
        var matched = Collections.Filter(items, (CappedOffboardingProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped offboarding profile store.</summary>
public static class CappedOffboardingProfileStore
{
    public static ICappedOffboardingProfileStore NewMemoryCappedOffboardingProfileStore()
    {
        return new MemoryCappedOffboardingProfileStore(
            new Dictionary<string, CappedOffboardingProfile>(),
            new List<string>());
    }

    public static ICappedOffboardingProfileStore NewCachedCappedOffboardingProfileStore(MemoryCappedOffboardingProfileStore inner, int ttl, IClock clock)
    {
        return new CachedCappedOffboardingProfileStore(
            inner,
            CappedOffboardingProfileCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CappedOffboardingProfile> FindCappedOffboardingProfiles(ICappedOffboardingProfileStore store, CappedOffboardingProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
