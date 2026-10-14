namespace Acme.Workforce.Payrollops.PayRun.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run mappings by code.</summary>
public interface IPayRunMappingStore
{
    PayRunMapping? Find(string code);
    void Save(PayRunMapping item);
    bool Remove(string code);
    List<PayRunMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunMappingStore(Dictionary<string, PayRunMapping> items, List<string> order) : IPayRunMappingStore
{
    public Dictionary<string, PayRunMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunMapping item)
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

    public List<PayRunMapping> List()
    {
        var outValue = new List<PayRunMapping>();
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
public sealed class CachedPayRunMappingStore(MemoryPayRunMappingStore inner, PayRunMappingCache cache, int hits, int misses) : IPayRunMappingStore
{
    public MemoryPayRunMappingStore Inner { get; set; } = inner;
    public PayRunMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunMapping? Find(string code)
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

    public void Save(PayRunMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run mappings. Empty fields match all.</summary>
public sealed record PayRunMappingQuery(string Name, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunMapping item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayRunMapping> Apply(List<PayRunMapping> items)
    {
        var matched = Collections.Filter(items, (PayRunMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run mapping store.</summary>
public static class PayRunMappingStore
{
    public static IPayRunMappingStore NewMemoryPayRunMappingStore()
    {
        return new MemoryPayRunMappingStore(new Dictionary<string, PayRunMapping>(), new List<string>());
    }

    public static IPayRunMappingStore NewCachedPayRunMappingStore(MemoryPayRunMappingStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunMappingStore(inner, PayRunMappingCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PayRunMapping> FindPayRunMappings(IPayRunMappingStore store, PayRunMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
