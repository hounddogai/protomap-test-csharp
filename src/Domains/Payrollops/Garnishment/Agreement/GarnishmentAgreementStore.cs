namespace Acme.Workforce.Payrollops.Garnishment.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment agreements by code.</summary>
public interface IGarnishmentAgreementStore
{
    GarnishmentAgreement? Find(string code);
    void Save(GarnishmentAgreement item);
    bool Remove(string code);
    List<GarnishmentAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentAgreementStore(Dictionary<string, GarnishmentAgreement> items, List<string> order) : IGarnishmentAgreementStore
{
    public Dictionary<string, GarnishmentAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentAgreement item)
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

    public List<GarnishmentAgreement> List()
    {
        var outValue = new List<GarnishmentAgreement>();
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
public sealed class CachedGarnishmentAgreementStore(MemoryGarnishmentAgreementStore inner, GarnishmentAgreementCache cache, int hits, int misses) : IGarnishmentAgreementStore
{
    public MemoryGarnishmentAgreementStore Inner { get; set; } = inner;
    public GarnishmentAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentAgreement? Find(string code)
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

    public void Save(GarnishmentAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment agreements. Empty fields match all.</summary>
public sealed record GarnishmentAgreementQuery(string Title, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentAgreement item)
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

    public List<GarnishmentAgreement> Apply(List<GarnishmentAgreement> items)
    {
        var matched = Collections.Filter(items, (GarnishmentAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment agreement store.</summary>
public static class GarnishmentAgreementStore
{
    public static IGarnishmentAgreementStore NewMemoryGarnishmentAgreementStore()
    {
        return new MemoryGarnishmentAgreementStore(
            new Dictionary<string, GarnishmentAgreement>(),
            new List<string>());
    }

    public static IGarnishmentAgreementStore NewCachedGarnishmentAgreementStore(MemoryGarnishmentAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentAgreementStore(inner, GarnishmentAgreementCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<GarnishmentAgreement> FindGarnishmentAgreements(IGarnishmentAgreementStore store, GarnishmentAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
