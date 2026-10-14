namespace Acme.Workforce.Talent.CareerPath.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path thresholds by code.</summary>
public interface ICareerPathThresholdStore
{
    CareerPathThreshold? Find(string code);
    void Save(CareerPathThreshold item);
    bool Remove(string code);
    List<CareerPathThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathThresholdStore(Dictionary<string, CareerPathThreshold> items, List<string> order) : ICareerPathThresholdStore
{
    public Dictionary<string, CareerPathThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathThreshold item)
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

    public List<CareerPathThreshold> List()
    {
        var outValue = new List<CareerPathThreshold>();
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
public sealed class CachedCareerPathThresholdStore(MemoryCareerPathThresholdStore inner, CareerPathThresholdCache cache, int hits, int misses) : ICareerPathThresholdStore
{
    public MemoryCareerPathThresholdStore Inner { get; set; } = inner;
    public CareerPathThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathThreshold? Find(string code)
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

    public void Save(CareerPathThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path thresholds. Empty fields match all.</summary>
public sealed record CareerPathThresholdQuery(string Label, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathThreshold item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathThreshold> Apply(List<CareerPathThreshold> items)
    {
        var matched = Collections.Filter(items, (CareerPathThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path threshold store.</summary>
public static class CareerPathThresholdStore
{
    public static ICareerPathThresholdStore NewMemoryCareerPathThresholdStore()
    {
        return new MemoryCareerPathThresholdStore(new Dictionary<string, CareerPathThreshold>(), new List<string>());
    }

    public static ICareerPathThresholdStore NewCachedCareerPathThresholdStore(MemoryCareerPathThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathThresholdStore(inner, CareerPathThresholdCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CareerPathThreshold> FindCareerPathThresholds(ICareerPathThresholdStore store, CareerPathThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
