namespace Acme.Workforce.Talent.CareerPath.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path allocations by code.</summary>
public interface ICareerPathAllocationStore
{
    CareerPathAllocation? Find(string code);
    void Save(CareerPathAllocation item);
    bool Remove(string code);
    List<CareerPathAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathAllocationStore(Dictionary<string, CareerPathAllocation> items, List<string> order) : ICareerPathAllocationStore
{
    public Dictionary<string, CareerPathAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathAllocation item)
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

    public List<CareerPathAllocation> List()
    {
        var outValue = new List<CareerPathAllocation>();
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
public sealed class CachedCareerPathAllocationStore(MemoryCareerPathAllocationStore inner, CareerPathAllocationCache cache, int hits, int misses) : ICareerPathAllocationStore
{
    public MemoryCareerPathAllocationStore Inner { get; set; } = inner;
    public CareerPathAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathAllocation? Find(string code)
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

    public void Save(CareerPathAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path allocations. Empty fields match all.</summary>
public sealed record CareerPathAllocationQuery(string Channel, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathAllocation item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathAllocation> Apply(List<CareerPathAllocation> items)
    {
        var matched = Collections.Filter(items, (CareerPathAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path allocation store.</summary>
public static class CareerPathAllocationStore
{
    public static ICareerPathAllocationStore NewMemoryCareerPathAllocationStore()
    {
        return new MemoryCareerPathAllocationStore(
            new Dictionary<string, CareerPathAllocation>(),
            new List<string>());
    }

    public static ICareerPathAllocationStore NewCachedCareerPathAllocationStore(MemoryCareerPathAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathAllocationStore(inner, CareerPathAllocationCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<CareerPathAllocation> FindCareerPathAllocations(ICareerPathAllocationStore store, CareerPathAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
