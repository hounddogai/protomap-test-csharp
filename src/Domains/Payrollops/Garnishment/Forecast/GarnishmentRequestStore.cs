namespace Acme.Workforce.Payrollops.Garnishment.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment requests by code.</summary>
public interface IGarnishmentRequestStore
{
    GarnishmentRequest? Find(string code);
    void Save(GarnishmentRequest item);
    bool Remove(string code);
    List<GarnishmentRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentRequestStore(Dictionary<string, GarnishmentRequest> items, List<string> order) : IGarnishmentRequestStore
{
    public Dictionary<string, GarnishmentRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentRequest item)
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

    public List<GarnishmentRequest> List()
    {
        var outValue = new List<GarnishmentRequest>();
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
public sealed class CachedGarnishmentRequestStore(MemoryGarnishmentRequestStore inner, GarnishmentRequestCache cache, int hits, int misses) : IGarnishmentRequestStore
{
    public MemoryGarnishmentRequestStore Inner { get; set; } = inner;
    public GarnishmentRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentRequest? Find(string code)
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

    public void Save(GarnishmentRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment requests. Empty fields match all.</summary>
public sealed record GarnishmentRequestQuery(string Title, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentRequest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GarnishmentRequest> Apply(List<GarnishmentRequest> items)
    {
        var matched = Collections.Filter(items, (GarnishmentRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment request store.</summary>
public static class GarnishmentRequestStore
{
    public static IGarnishmentRequestStore NewMemoryGarnishmentRequestStore()
    {
        return new MemoryGarnishmentRequestStore(new Dictionary<string, GarnishmentRequest>(), new List<string>());
    }

    public static IGarnishmentRequestStore NewCachedGarnishmentRequestStore(MemoryGarnishmentRequestStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentRequestStore(inner, GarnishmentRequestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<GarnishmentRequest> FindGarnishmentRequests(IGarnishmentRequestStore store, GarnishmentRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
