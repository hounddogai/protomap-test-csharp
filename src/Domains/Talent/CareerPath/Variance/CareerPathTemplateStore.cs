namespace Acme.Workforce.Talent.CareerPath.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path templates by code.</summary>
public interface ICareerPathTemplateStore
{
    CareerPathTemplate? Find(string code);
    void Save(CareerPathTemplate item);
    bool Remove(string code);
    List<CareerPathTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathTemplateStore(Dictionary<string, CareerPathTemplate> items, List<string> order) : ICareerPathTemplateStore
{
    public Dictionary<string, CareerPathTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathTemplate item)
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

    public List<CareerPathTemplate> List()
    {
        var outValue = new List<CareerPathTemplate>();
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
public sealed class CachedCareerPathTemplateStore(MemoryCareerPathTemplateStore inner, CareerPathTemplateCache cache, int hits, int misses) : ICareerPathTemplateStore
{
    public MemoryCareerPathTemplateStore Inner { get; set; } = inner;
    public CareerPathTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathTemplate? Find(string code)
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

    public void Save(CareerPathTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path templates. Empty fields match all.</summary>
public sealed record CareerPathTemplateQuery(string Name, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathTemplate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathTemplate> Apply(List<CareerPathTemplate> items)
    {
        var matched = Collections.Filter(items, (CareerPathTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path template store.</summary>
public static class CareerPathTemplateStore
{
    public static ICareerPathTemplateStore NewMemoryCareerPathTemplateStore()
    {
        return new MemoryCareerPathTemplateStore(new Dictionary<string, CareerPathTemplate>(), new List<string>());
    }

    public static ICareerPathTemplateStore NewCachedCareerPathTemplateStore(MemoryCareerPathTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathTemplateStore(inner, CareerPathTemplateCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CareerPathTemplate> FindCareerPathTemplates(ICareerPathTemplateStore store, CareerPathTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
