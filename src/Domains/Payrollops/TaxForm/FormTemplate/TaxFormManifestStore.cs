namespace Acme.Workforce.Payrollops.TaxForm.FormTemplate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form manifests by code.</summary>
public interface ITaxFormManifestStore
{
    TaxFormManifest? Find(string code);
    void Save(TaxFormManifest item);
    bool Remove(string code);
    List<TaxFormManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormManifestStore(Dictionary<string, TaxFormManifest> items, List<string> order) : ITaxFormManifestStore
{
    public Dictionary<string, TaxFormManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormManifest item)
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

    public List<TaxFormManifest> List()
    {
        var outValue = new List<TaxFormManifest>();
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
public sealed class CachedTaxFormManifestStore(MemoryTaxFormManifestStore inner, TaxFormManifestCache cache, int hits, int misses) : ITaxFormManifestStore
{
    public MemoryTaxFormManifestStore Inner { get; set; } = inner;
    public TaxFormManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormManifest? Find(string code)
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

    public void Save(TaxFormManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form manifests. Empty fields match all.</summary>
public sealed record TaxFormManifestQuery(string Title, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormManifest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TaxFormManifest> Apply(List<TaxFormManifest> items)
    {
        var matched = Collections.Filter(items, (TaxFormManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form manifest store.</summary>
public static class TaxFormManifestStore
{
    public static ITaxFormManifestStore NewMemoryTaxFormManifestStore()
    {
        return new MemoryTaxFormManifestStore(new Dictionary<string, TaxFormManifest>(), new List<string>());
    }

    public static ITaxFormManifestStore NewCachedTaxFormManifestStore(MemoryTaxFormManifestStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormManifestStore(inner, TaxFormManifestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<TaxFormManifest> FindTaxFormManifests(ITaxFormManifestStore store, TaxFormManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
