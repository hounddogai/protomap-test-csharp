namespace Acme.Workforce.Talent.CareerPath.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path entries by code.</summary>
public interface ICareerPathEntryStore
{
    CareerPathEntry? Find(string code);
    void Save(CareerPathEntry item);
    bool Remove(string code);
    List<CareerPathEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathEntryStore(Dictionary<string, CareerPathEntry> items, List<string> order) : ICareerPathEntryStore
{
    public Dictionary<string, CareerPathEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathEntry item)
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

    public List<CareerPathEntry> List()
    {
        var outValue = new List<CareerPathEntry>();
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
public sealed class CachedCareerPathEntryStore(MemoryCareerPathEntryStore inner, CareerPathEntryCache cache, int hits, int misses) : ICareerPathEntryStore
{
    public MemoryCareerPathEntryStore Inner { get; set; } = inner;
    public CareerPathEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathEntry? Find(string code)
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

    public void Save(CareerPathEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path entries. Empty fields match all.</summary>
public sealed record CareerPathEntryQuery(string Category, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathEntry item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<CareerPathEntry> Apply(List<CareerPathEntry> items)
    {
        var matched = Collections.Filter(items, (CareerPathEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path entry store.</summary>
public static class CareerPathEntryStore
{
    public static ICareerPathEntryStore NewMemoryCareerPathEntryStore()
    {
        return new MemoryCareerPathEntryStore(new Dictionary<string, CareerPathEntry>(), new List<string>());
    }

    public static ICareerPathEntryStore NewCachedCareerPathEntryStore(MemoryCareerPathEntryStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathEntryStore(inner, CareerPathEntryCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CareerPathEntry> FindCareerPathEntries(ICareerPathEntryStore store, CareerPathEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
