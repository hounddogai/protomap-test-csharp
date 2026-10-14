namespace Acme.Workforce.Talent.CareerPath.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path assignments by code.</summary>
public interface ICareerPathAssignmentStore
{
    CareerPathAssignment? Find(string code);
    void Save(CareerPathAssignment item);
    bool Remove(string code);
    List<CareerPathAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathAssignmentStore(Dictionary<string, CareerPathAssignment> items, List<string> order) : ICareerPathAssignmentStore
{
    public Dictionary<string, CareerPathAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathAssignment item)
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

    public List<CareerPathAssignment> List()
    {
        var outValue = new List<CareerPathAssignment>();
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
public sealed class CachedCareerPathAssignmentStore(MemoryCareerPathAssignmentStore inner, CareerPathAssignmentCache cache, int hits, int misses) : ICareerPathAssignmentStore
{
    public MemoryCareerPathAssignmentStore Inner { get; set; } = inner;
    public CareerPathAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathAssignment? Find(string code)
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

    public void Save(CareerPathAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path assignments. Empty fields match all.</summary>
public sealed record CareerPathAssignmentQuery(string Category, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathAssignment item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathAssignment> Apply(List<CareerPathAssignment> items)
    {
        var matched = Collections.Filter(items, (CareerPathAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path assignment store.</summary>
public static class CareerPathAssignmentStore
{
    public static ICareerPathAssignmentStore NewMemoryCareerPathAssignmentStore()
    {
        return new MemoryCareerPathAssignmentStore(
            new Dictionary<string, CareerPathAssignment>(),
            new List<string>());
    }

    public static ICareerPathAssignmentStore NewCachedCareerPathAssignmentStore(MemoryCareerPathAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathAssignmentStore(inner, CareerPathAssignmentCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CareerPathAssignment> FindCareerPathAssignments(ICareerPathAssignmentStore store, CareerPathAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
