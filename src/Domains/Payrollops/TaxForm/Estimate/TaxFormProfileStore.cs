namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form profiles by code.</summary>
public interface ITaxFormProfileStore
{
    TaxFormProfile? Find(string code);
    void Save(TaxFormProfile item);
    bool Remove(string code);
    List<TaxFormProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormProfileStore(Dictionary<string, TaxFormProfile> items, List<string> order) : ITaxFormProfileStore
{
    public Dictionary<string, TaxFormProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormProfile item)
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

    public List<TaxFormProfile> List()
    {
        var outValue = new List<TaxFormProfile>();
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
public sealed class CachedTaxFormProfileStore(MemoryTaxFormProfileStore inner, TaxFormProfileCache cache, int hits, int misses) : ITaxFormProfileStore
{
    public MemoryTaxFormProfileStore Inner { get; set; } = inner;
    public TaxFormProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormProfile? Find(string code)
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

    public void Save(TaxFormProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form profiles. Empty fields match all.</summary>
public sealed record TaxFormProfileQuery(string Locale, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormProfile item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TaxFormProfile> Apply(List<TaxFormProfile> items)
    {
        var matched = Collections.Filter(items, (TaxFormProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form profile store.</summary>
public static class TaxFormProfileStore
{
    public static ITaxFormProfileStore NewMemoryTaxFormProfileStore()
    {
        return new MemoryTaxFormProfileStore(new Dictionary<string, TaxFormProfile>(), new List<string>());
    }

    public static ITaxFormProfileStore NewCachedTaxFormProfileStore(MemoryTaxFormProfileStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormProfileStore(inner, TaxFormProfileCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TaxFormProfile> FindTaxFormProfiles(ITaxFormProfileStore store, TaxFormProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
