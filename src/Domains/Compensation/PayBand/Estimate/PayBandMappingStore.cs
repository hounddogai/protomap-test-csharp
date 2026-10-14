namespace Acme.Workforce.Compensation.PayBand.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band mappings by code.</summary>
public interface IPayBandMappingStore
{
    PayBandMapping? Find(string code);
    void Save(PayBandMapping item);
    bool Remove(string code);
    List<PayBandMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandMappingStore(Dictionary<string, PayBandMapping> items, List<string> order) : IPayBandMappingStore
{
    public Dictionary<string, PayBandMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandMapping item)
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

    public List<PayBandMapping> List()
    {
        var outValue = new List<PayBandMapping>();
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
public sealed class CachedPayBandMappingStore(MemoryPayBandMappingStore inner, PayBandMappingCache cache, int hits, int misses) : IPayBandMappingStore
{
    public MemoryPayBandMappingStore Inner { get; set; } = inner;
    public PayBandMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandMapping? Find(string code)
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

    public void Save(PayBandMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band mappings. Empty fields match all.</summary>
public sealed record PayBandMappingQuery(string Label, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandMapping item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandMapping> Apply(List<PayBandMapping> items)
    {
        var matched = Collections.Filter(items, (PayBandMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band mapping store.</summary>
public static class PayBandMappingStore
{
    public static IPayBandMappingStore NewMemoryPayBandMappingStore()
    {
        return new MemoryPayBandMappingStore(new Dictionary<string, PayBandMapping>(), new List<string>());
    }

    public static IPayBandMappingStore NewCachedPayBandMappingStore(MemoryPayBandMappingStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandMappingStore(inner, PayBandMappingCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PayBandMapping> FindPayBandMappings(IPayBandMappingStore store, PayBandMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
