namespace Acme.Workforce.Talent.CareerPath.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path overrides by code.</summary>
public interface ICareerPathOverrideStore
{
    CareerPathOverride? Find(string code);
    void Save(CareerPathOverride item);
    bool Remove(string code);
    List<CareerPathOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathOverrideStore(Dictionary<string, CareerPathOverride> items, List<string> order) : ICareerPathOverrideStore
{
    public Dictionary<string, CareerPathOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathOverride item)
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

    public List<CareerPathOverride> List()
    {
        var outValue = new List<CareerPathOverride>();
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
public sealed class CachedCareerPathOverrideStore(MemoryCareerPathOverrideStore inner, CareerPathOverrideCache cache, int hits, int misses) : ICareerPathOverrideStore
{
    public MemoryCareerPathOverrideStore Inner { get; set; } = inner;
    public CareerPathOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathOverride? Find(string code)
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

    public void Save(CareerPathOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path overrides. Empty fields match all.</summary>
public sealed record CareerPathOverrideQuery(string Channel, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathOverride item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<CareerPathOverride> Apply(List<CareerPathOverride> items)
    {
        var matched = Collections.Filter(items, (CareerPathOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path override store.</summary>
public static class CareerPathOverrideStore
{
    public static ICareerPathOverrideStore NewMemoryCareerPathOverrideStore()
    {
        return new MemoryCareerPathOverrideStore(new Dictionary<string, CareerPathOverride>(), new List<string>());
    }

    public static ICareerPathOverrideStore NewCachedCareerPathOverrideStore(MemoryCareerPathOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathOverrideStore(inner, CareerPathOverrideCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<CareerPathOverride> FindCareerPathOverrides(ICareerPathOverrideStore store, CareerPathOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
