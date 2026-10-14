namespace Acme.Workforce.Payrollops.Garnishment.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted garnishment requests by code.</summary>
public interface IWeightedGarnishmentRequestStore
{
    WeightedGarnishmentRequest? Find(string code);
    void Save(WeightedGarnishmentRequest item);
    bool Remove(string code);
    List<WeightedGarnishmentRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedGarnishmentRequestStore(Dictionary<string, WeightedGarnishmentRequest> items, List<string> order) : IWeightedGarnishmentRequestStore
{
    public Dictionary<string, WeightedGarnishmentRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedGarnishmentRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedGarnishmentRequest item)
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

    public List<WeightedGarnishmentRequest> List()
    {
        var outValue = new List<WeightedGarnishmentRequest>();
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
public sealed class CachedWeightedGarnishmentRequestStore(MemoryWeightedGarnishmentRequestStore inner, WeightedGarnishmentRequestCache cache, int hits, int misses) : IWeightedGarnishmentRequestStore
{
    public MemoryWeightedGarnishmentRequestStore Inner { get; set; } = inner;
    public WeightedGarnishmentRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedGarnishmentRequest? Find(string code)
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

    public void Save(WeightedGarnishmentRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedGarnishmentRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted garnishment requests. Empty fields match all.</summary>
public sealed record WeightedGarnishmentRequestQuery(string Channel, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedGarnishmentRequest item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<WeightedGarnishmentRequest> Apply(List<WeightedGarnishmentRequest> items)
    {
        var matched = Collections.Filter(items, (WeightedGarnishmentRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted garnishment request store.</summary>
public static class WeightedGarnishmentRequestStore
{
    public static IWeightedGarnishmentRequestStore NewMemoryWeightedGarnishmentRequestStore()
    {
        return new MemoryWeightedGarnishmentRequestStore(
            new Dictionary<string, WeightedGarnishmentRequest>(),
            new List<string>());
    }

    public static IWeightedGarnishmentRequestStore NewCachedWeightedGarnishmentRequestStore(MemoryWeightedGarnishmentRequestStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedGarnishmentRequestStore(
            inner,
            WeightedGarnishmentRequestCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<WeightedGarnishmentRequest> FindWeightedGarnishmentRequests(IWeightedGarnishmentRequestStore store, WeightedGarnishmentRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
