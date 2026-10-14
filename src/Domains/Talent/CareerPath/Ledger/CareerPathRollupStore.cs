namespace Acme.Workforce.Talent.CareerPath.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path rollups by code.</summary>
public interface ICareerPathRollupStore
{
    CareerPathRollup? Find(string code);
    void Save(CareerPathRollup item);
    bool Remove(string code);
    List<CareerPathRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathRollupStore(Dictionary<string, CareerPathRollup> items, List<string> order) : ICareerPathRollupStore
{
    public Dictionary<string, CareerPathRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathRollup item)
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

    public List<CareerPathRollup> List()
    {
        var outValue = new List<CareerPathRollup>();
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
public sealed class CachedCareerPathRollupStore(MemoryCareerPathRollupStore inner, CareerPathRollupCache cache, int hits, int misses) : ICareerPathRollupStore
{
    public MemoryCareerPathRollupStore Inner { get; set; } = inner;
    public CareerPathRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathRollup? Find(string code)
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

    public void Save(CareerPathRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path rollups. Empty fields match all.</summary>
public sealed record CareerPathRollupQuery(string Locale, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathRollup item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathRollup> Apply(List<CareerPathRollup> items)
    {
        var matched = Collections.Filter(items, (CareerPathRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path rollup store.</summary>
public static class CareerPathRollupStore
{
    public static ICareerPathRollupStore NewMemoryCareerPathRollupStore()
    {
        return new MemoryCareerPathRollupStore(new Dictionary<string, CareerPathRollup>(), new List<string>());
    }

    public static ICareerPathRollupStore NewCachedCareerPathRollupStore(MemoryCareerPathRollupStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathRollupStore(inner, CareerPathRollupCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CareerPathRollup> FindCareerPathRollups(ICareerPathRollupStore store, CareerPathRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
