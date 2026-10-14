namespace Acme.Workforce.Talent.CareerPath.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path policies by code.</summary>
public interface ICareerPathPolicyStore
{
    CareerPathPolicy? Find(string code);
    void Save(CareerPathPolicy item);
    bool Remove(string code);
    List<CareerPathPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathPolicyStore(Dictionary<string, CareerPathPolicy> items, List<string> order) : ICareerPathPolicyStore
{
    public Dictionary<string, CareerPathPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathPolicy item)
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

    public List<CareerPathPolicy> List()
    {
        var outValue = new List<CareerPathPolicy>();
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
public sealed class CachedCareerPathPolicyStore(MemoryCareerPathPolicyStore inner, CareerPathPolicyCache cache, int hits, int misses) : ICareerPathPolicyStore
{
    public MemoryCareerPathPolicyStore Inner { get; set; } = inner;
    public CareerPathPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathPolicy? Find(string code)
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

    public void Save(CareerPathPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path policies. Empty fields match all.</summary>
public sealed record CareerPathPolicyQuery(string Segment, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathPolicy item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathPolicy> Apply(List<CareerPathPolicy> items)
    {
        var matched = Collections.Filter(items, (CareerPathPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path policy store.</summary>
public static class CareerPathPolicyStore
{
    public static ICareerPathPolicyStore NewMemoryCareerPathPolicyStore()
    {
        return new MemoryCareerPathPolicyStore(new Dictionary<string, CareerPathPolicy>(), new List<string>());
    }

    public static ICareerPathPolicyStore NewCachedCareerPathPolicyStore(MemoryCareerPathPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathPolicyStore(inner, CareerPathPolicyCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<CareerPathPolicy> FindCareerPathPolicies(ICareerPathPolicyStore store, CareerPathPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
