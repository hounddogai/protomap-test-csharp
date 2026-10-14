namespace Acme.Workforce.Payrollops.Deduction.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped deduction groups by code.</summary>
public interface ICappedDeductionGroupStore
{
    CappedDeductionGroup? Find(string code);
    void Save(CappedDeductionGroup item);
    bool Remove(string code);
    List<CappedDeductionGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedDeductionGroupStore(Dictionary<string, CappedDeductionGroup> items, List<string> order) : ICappedDeductionGroupStore
{
    public Dictionary<string, CappedDeductionGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedDeductionGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedDeductionGroup item)
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

    public List<CappedDeductionGroup> List()
    {
        var outValue = new List<CappedDeductionGroup>();
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
public sealed class CachedCappedDeductionGroupStore(MemoryCappedDeductionGroupStore inner, CappedDeductionGroupCache cache, int hits, int misses) : ICappedDeductionGroupStore
{
    public MemoryCappedDeductionGroupStore Inner { get; set; } = inner;
    public CappedDeductionGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedDeductionGroup? Find(string code)
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

    public void Save(CappedDeductionGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedDeductionGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped deduction groups. Empty fields match all.</summary>
public sealed record CappedDeductionGroupQuery(string Region, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(CappedDeductionGroup item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedDeductionGroup> Apply(List<CappedDeductionGroup> items)
    {
        var matched = Collections.Filter(items, (CappedDeductionGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped deduction group store.</summary>
public static class CappedDeductionGroupStore
{
    public static ICappedDeductionGroupStore NewMemoryCappedDeductionGroupStore()
    {
        return new MemoryCappedDeductionGroupStore(
            new Dictionary<string, CappedDeductionGroup>(),
            new List<string>());
    }

    public static ICappedDeductionGroupStore NewCachedCappedDeductionGroupStore(MemoryCappedDeductionGroupStore inner, int ttl, IClock clock)
    {
        return new CachedCappedDeductionGroupStore(inner, CappedDeductionGroupCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<CappedDeductionGroup> FindCappedDeductionGroups(ICappedDeductionGroupStore store, CappedDeductionGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
