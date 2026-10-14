namespace Acme.Workforce.Payrollops.Deduction.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction groups by code.</summary>
public interface IDeductionGroupStore
{
    DeductionGroup? Find(string code);
    void Save(DeductionGroup item);
    bool Remove(string code);
    List<DeductionGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionGroupStore(Dictionary<string, DeductionGroup> items, List<string> order) : IDeductionGroupStore
{
    public Dictionary<string, DeductionGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionGroup item)
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

    public List<DeductionGroup> List()
    {
        var outValue = new List<DeductionGroup>();
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
public sealed class CachedDeductionGroupStore(MemoryDeductionGroupStore inner, DeductionGroupCache cache, int hits, int misses) : IDeductionGroupStore
{
    public MemoryDeductionGroupStore Inner { get; set; } = inner;
    public DeductionGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionGroup? Find(string code)
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

    public void Save(DeductionGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction groups. Empty fields match all.</summary>
public sealed record DeductionGroupQuery(string Channel, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionGroup item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeductionGroup> Apply(List<DeductionGroup> items)
    {
        var matched = Collections.Filter(items, (DeductionGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction group store.</summary>
public static class DeductionGroupStore
{
    public static IDeductionGroupStore NewMemoryDeductionGroupStore()
    {
        return new MemoryDeductionGroupStore(new Dictionary<string, DeductionGroup>(), new List<string>());
    }

    public static IDeductionGroupStore NewCachedDeductionGroupStore(MemoryDeductionGroupStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionGroupStore(inner, DeductionGroupCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DeductionGroup> FindDeductionGroups(IDeductionGroupStore store, DeductionGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
