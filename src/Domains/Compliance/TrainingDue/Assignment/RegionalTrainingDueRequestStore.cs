namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional training due requests by code.</summary>
public interface IRegionalTrainingDueRequestStore
{
    RegionalTrainingDueRequest? Find(string code);
    void Save(RegionalTrainingDueRequest item);
    bool Remove(string code);
    List<RegionalTrainingDueRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalTrainingDueRequestStore(Dictionary<string, RegionalTrainingDueRequest> items, List<string> order) : IRegionalTrainingDueRequestStore
{
    public Dictionary<string, RegionalTrainingDueRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalTrainingDueRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalTrainingDueRequest item)
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

    public List<RegionalTrainingDueRequest> List()
    {
        var outValue = new List<RegionalTrainingDueRequest>();
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
public sealed class CachedRegionalTrainingDueRequestStore(MemoryRegionalTrainingDueRequestStore inner, RegionalTrainingDueRequestCache cache, int hits, int misses) : IRegionalTrainingDueRequestStore
{
    public MemoryRegionalTrainingDueRequestStore Inner { get; set; } = inner;
    public RegionalTrainingDueRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalTrainingDueRequest? Find(string code)
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

    public void Save(RegionalTrainingDueRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalTrainingDueRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional training due requests. Empty fields match all.</summary>
public sealed record RegionalTrainingDueRequestQuery(string Label, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalTrainingDueRequest item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalTrainingDueRequest> Apply(List<RegionalTrainingDueRequest> items)
    {
        var matched = Collections.Filter(items, (RegionalTrainingDueRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional training due request store.</summary>
public static class RegionalTrainingDueRequestStore
{
    public static IRegionalTrainingDueRequestStore NewMemoryRegionalTrainingDueRequestStore()
    {
        return new MemoryRegionalTrainingDueRequestStore(
            new Dictionary<string, RegionalTrainingDueRequest>(),
            new List<string>());
    }

    public static IRegionalTrainingDueRequestStore NewCachedRegionalTrainingDueRequestStore(MemoryRegionalTrainingDueRequestStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalTrainingDueRequestStore(
            inner,
            RegionalTrainingDueRequestCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<RegionalTrainingDueRequest> FindRegionalTrainingDueRequests(IRegionalTrainingDueRequestStore store, RegionalTrainingDueRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
