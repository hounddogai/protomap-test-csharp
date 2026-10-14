namespace Acme.Workforce.Talent.CareerPath.PathDigest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path estimates by code.</summary>
public interface ICareerPathEstimateStore
{
    CareerPathEstimate? Find(string code);
    void Save(CareerPathEstimate item);
    bool Remove(string code);
    List<CareerPathEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathEstimateStore(Dictionary<string, CareerPathEstimate> items, List<string> order) : ICareerPathEstimateStore
{
    public Dictionary<string, CareerPathEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathEstimate item)
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

    public List<CareerPathEstimate> List()
    {
        var outValue = new List<CareerPathEstimate>();
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
public sealed class CachedCareerPathEstimateStore(MemoryCareerPathEstimateStore inner, CareerPathEstimateCache cache, int hits, int misses) : ICareerPathEstimateStore
{
    public MemoryCareerPathEstimateStore Inner { get; set; } = inner;
    public CareerPathEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathEstimate? Find(string code)
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

    public void Save(CareerPathEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path estimates. Empty fields match all.</summary>
public sealed record CareerPathEstimateQuery(string Unit, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathEstimate item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathEstimate> Apply(List<CareerPathEstimate> items)
    {
        var matched = Collections.Filter(items, (CareerPathEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path estimate store.</summary>
public static class CareerPathEstimateStore
{
    public static ICareerPathEstimateStore NewMemoryCareerPathEstimateStore()
    {
        return new MemoryCareerPathEstimateStore(new Dictionary<string, CareerPathEstimate>(), new List<string>());
    }

    public static ICareerPathEstimateStore NewCachedCareerPathEstimateStore(MemoryCareerPathEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathEstimateStore(inner, CareerPathEstimateCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<CareerPathEstimate> FindCareerPathEstimates(ICareerPathEstimateStore store, CareerPathEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
