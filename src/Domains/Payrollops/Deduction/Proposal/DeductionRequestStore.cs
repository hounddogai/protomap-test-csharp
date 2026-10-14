namespace Acme.Workforce.Payrollops.Deduction.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction requests by code.</summary>
public interface IDeductionRequestStore
{
    DeductionRequest? Find(string code);
    void Save(DeductionRequest item);
    bool Remove(string code);
    List<DeductionRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionRequestStore(Dictionary<string, DeductionRequest> items, List<string> order) : IDeductionRequestStore
{
    public Dictionary<string, DeductionRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionRequest item)
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

    public List<DeductionRequest> List()
    {
        var outValue = new List<DeductionRequest>();
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
public sealed class CachedDeductionRequestStore(MemoryDeductionRequestStore inner, DeductionRequestCache cache, int hits, int misses) : IDeductionRequestStore
{
    public MemoryDeductionRequestStore Inner { get; set; } = inner;
    public DeductionRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionRequest? Find(string code)
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

    public void Save(DeductionRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction requests. Empty fields match all.</summary>
public sealed record DeductionRequestQuery(string Country, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionRequest item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<DeductionRequest> Apply(List<DeductionRequest> items)
    {
        var matched = Collections.Filter(items, (DeductionRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction request store.</summary>
public static class DeductionRequestStore
{
    public static IDeductionRequestStore NewMemoryDeductionRequestStore()
    {
        return new MemoryDeductionRequestStore(new Dictionary<string, DeductionRequest>(), new List<string>());
    }

    public static IDeductionRequestStore NewCachedDeductionRequestStore(MemoryDeductionRequestStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionRequestStore(inner, DeductionRequestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<DeductionRequest> FindDeductionRequests(IDeductionRequestStore store, DeductionRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
