namespace Acme.Workforce.Contractors.Rate.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate profiles by code.</summary>
public interface IRateProfileStore
{
    RateProfile? Find(string code);
    void Save(RateProfile item);
    bool Remove(string code);
    List<RateProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateProfileStore(Dictionary<string, RateProfile> items, List<string> order) : IRateProfileStore
{
    public Dictionary<string, RateProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateProfile item)
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

    public List<RateProfile> List()
    {
        var outValue = new List<RateProfile>();
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
public sealed class CachedRateProfileStore(MemoryRateProfileStore inner, RateProfileCache cache, int hits, int misses) : IRateProfileStore
{
    public MemoryRateProfileStore Inner { get; set; } = inner;
    public RateProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateProfile? Find(string code)
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

    public void Save(RateProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate profiles. Empty fields match all.</summary>
public sealed record RateProfileQuery(string Channel, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(RateProfile item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RateProfile> Apply(List<RateProfile> items)
    {
        var matched = Collections.Filter(items, (RateProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate profile store.</summary>
public static class RateProfileStore
{
    public static IRateProfileStore NewMemoryRateProfileStore()
    {
        return new MemoryRateProfileStore(new Dictionary<string, RateProfile>(), new List<string>());
    }

    public static IRateProfileStore NewCachedRateProfileStore(MemoryRateProfileStore inner, int ttl, IClock clock)
    {
        return new CachedRateProfileStore(inner, RateProfileCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RateProfile> FindRateProfiles(IRateProfileStore store, RateProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
