namespace Acme.Workforce.Talent.Nomination.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict nomination profiles by code.</summary>
public interface IStrictNominationProfileStore
{
    StrictNominationProfile? Find(string code);
    void Save(StrictNominationProfile item);
    bool Remove(string code);
    List<StrictNominationProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictNominationProfileStore(Dictionary<string, StrictNominationProfile> items, List<string> order) : IStrictNominationProfileStore
{
    public Dictionary<string, StrictNominationProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictNominationProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictNominationProfile item)
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

    public List<StrictNominationProfile> List()
    {
        var outValue = new List<StrictNominationProfile>();
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
public sealed class CachedStrictNominationProfileStore(MemoryStrictNominationProfileStore inner, StrictNominationProfileCache cache, int hits, int misses) : IStrictNominationProfileStore
{
    public MemoryStrictNominationProfileStore Inner { get; set; } = inner;
    public StrictNominationProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictNominationProfile? Find(string code)
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

    public void Save(StrictNominationProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictNominationProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict nomination profiles. Empty fields match all.</summary>
public sealed record StrictNominationProfileQuery(string Unit, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(StrictNominationProfile item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictNominationProfile> Apply(List<StrictNominationProfile> items)
    {
        var matched = Collections.Filter(items, (StrictNominationProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict nomination profile store.</summary>
public static class StrictNominationProfileStore
{
    public static IStrictNominationProfileStore NewMemoryStrictNominationProfileStore()
    {
        return new MemoryStrictNominationProfileStore(
            new Dictionary<string, StrictNominationProfile>(),
            new List<string>());
    }

    public static IStrictNominationProfileStore NewCachedStrictNominationProfileStore(MemoryStrictNominationProfileStore inner, int ttl, IClock clock)
    {
        return new CachedStrictNominationProfileStore(
            inner,
            StrictNominationProfileCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<StrictNominationProfile> FindStrictNominationProfiles(IStrictNominationProfileStore store, StrictNominationProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
