namespace Acme.Workforce.Payrollops.Deduction.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction mappings by code.</summary>
public interface IDeductionMappingStore
{
    DeductionMapping? Find(string code);
    void Save(DeductionMapping item);
    bool Remove(string code);
    List<DeductionMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionMappingStore(Dictionary<string, DeductionMapping> items, List<string> order) : IDeductionMappingStore
{
    public Dictionary<string, DeductionMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionMapping item)
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

    public List<DeductionMapping> List()
    {
        var outValue = new List<DeductionMapping>();
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
public sealed class CachedDeductionMappingStore(MemoryDeductionMappingStore inner, DeductionMappingCache cache, int hits, int misses) : IDeductionMappingStore
{
    public MemoryDeductionMappingStore Inner { get; set; } = inner;
    public DeductionMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionMapping? Find(string code)
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

    public void Save(DeductionMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction mappings. Empty fields match all.</summary>
public sealed record DeductionMappingQuery(string Team, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionMapping item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeductionMapping> Apply(List<DeductionMapping> items)
    {
        var matched = Collections.Filter(items, (DeductionMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction mapping store.</summary>
public static class DeductionMappingStore
{
    public static IDeductionMappingStore NewMemoryDeductionMappingStore()
    {
        return new MemoryDeductionMappingStore(new Dictionary<string, DeductionMapping>(), new List<string>());
    }

    public static IDeductionMappingStore NewCachedDeductionMappingStore(MemoryDeductionMappingStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionMappingStore(inner, DeductionMappingCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DeductionMapping> FindDeductionMappings(IDeductionMappingStore store, DeductionMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
