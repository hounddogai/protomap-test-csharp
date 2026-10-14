namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive tax form records by code.</summary>
public interface IAdaptiveTaxFormRecordStore
{
    AdaptiveTaxFormRecord? Find(string code);
    void Save(AdaptiveTaxFormRecord item);
    bool Remove(string code);
    List<AdaptiveTaxFormRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveTaxFormRecordStore(Dictionary<string, AdaptiveTaxFormRecord> items, List<string> order) : IAdaptiveTaxFormRecordStore
{
    public Dictionary<string, AdaptiveTaxFormRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveTaxFormRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveTaxFormRecord item)
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

    public List<AdaptiveTaxFormRecord> List()
    {
        var outValue = new List<AdaptiveTaxFormRecord>();
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
public sealed class CachedAdaptiveTaxFormRecordStore(MemoryAdaptiveTaxFormRecordStore inner, AdaptiveTaxFormRecordCache cache, int hits, int misses) : IAdaptiveTaxFormRecordStore
{
    public MemoryAdaptiveTaxFormRecordStore Inner { get; set; } = inner;
    public AdaptiveTaxFormRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveTaxFormRecord? Find(string code)
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

    public void Save(AdaptiveTaxFormRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveTaxFormRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive tax form records. Empty fields match all.</summary>
public sealed record AdaptiveTaxFormRecordQuery(string Channel, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveTaxFormRecord item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<AdaptiveTaxFormRecord> Apply(List<AdaptiveTaxFormRecord> items)
    {
        var matched = Collections.Filter(items, (AdaptiveTaxFormRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive tax form record store.</summary>
public static class AdaptiveTaxFormRecordStore
{
    public static IAdaptiveTaxFormRecordStore NewMemoryAdaptiveTaxFormRecordStore()
    {
        return new MemoryAdaptiveTaxFormRecordStore(
            new Dictionary<string, AdaptiveTaxFormRecord>(),
            new List<string>());
    }

    public static IAdaptiveTaxFormRecordStore NewCachedAdaptiveTaxFormRecordStore(MemoryAdaptiveTaxFormRecordStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveTaxFormRecordStore(inner, AdaptiveTaxFormRecordCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AdaptiveTaxFormRecord> FindAdaptiveTaxFormRecords(IAdaptiveTaxFormRecordStore store, AdaptiveTaxFormRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
