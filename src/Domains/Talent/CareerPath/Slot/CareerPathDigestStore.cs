namespace Acme.Workforce.Talent.CareerPath.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path digests by code.</summary>
public interface ICareerPathDigestStore
{
    CareerPathDigest? Find(string code);
    void Save(CareerPathDigest item);
    bool Remove(string code);
    List<CareerPathDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathDigestStore(Dictionary<string, CareerPathDigest> items, List<string> order) : ICareerPathDigestStore
{
    public Dictionary<string, CareerPathDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathDigest item)
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

    public List<CareerPathDigest> List()
    {
        var outValue = new List<CareerPathDigest>();
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
public sealed class CachedCareerPathDigestStore(MemoryCareerPathDigestStore inner, CareerPathDigestCache cache, int hits, int misses) : ICareerPathDigestStore
{
    public MemoryCareerPathDigestStore Inner { get; set; } = inner;
    public CareerPathDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathDigest? Find(string code)
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

    public void Save(CareerPathDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path digests. Empty fields match all.</summary>
public sealed record CareerPathDigestQuery(string Region, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathDigest item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathDigest> Apply(List<CareerPathDigest> items)
    {
        var matched = Collections.Filter(items, (CareerPathDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path digest store.</summary>
public static class CareerPathDigestStore
{
    public static ICareerPathDigestStore NewMemoryCareerPathDigestStore()
    {
        return new MemoryCareerPathDigestStore(new Dictionary<string, CareerPathDigest>(), new List<string>());
    }

    public static ICareerPathDigestStore NewCachedCareerPathDigestStore(MemoryCareerPathDigestStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathDigestStore(inner, CareerPathDigestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<CareerPathDigest> FindCareerPathDigests(ICareerPathDigestStore store, CareerPathDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
