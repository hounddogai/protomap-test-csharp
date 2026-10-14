namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional visitor baselines by code.</summary>
public interface IRegionalVisitorBaselineStore
{
    RegionalVisitorBaseline? Find(string code);
    void Save(RegionalVisitorBaseline item);
    bool Remove(string code);
    List<RegionalVisitorBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalVisitorBaselineStore(Dictionary<string, RegionalVisitorBaseline> items, List<string> order) : IRegionalVisitorBaselineStore
{
    public Dictionary<string, RegionalVisitorBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalVisitorBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalVisitorBaseline item)
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

    public List<RegionalVisitorBaseline> List()
    {
        var outValue = new List<RegionalVisitorBaseline>();
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
public sealed class CachedRegionalVisitorBaselineStore(MemoryRegionalVisitorBaselineStore inner, RegionalVisitorBaselineCache cache, int hits, int misses) : IRegionalVisitorBaselineStore
{
    public MemoryRegionalVisitorBaselineStore Inner { get; set; } = inner;
    public RegionalVisitorBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalVisitorBaseline? Find(string code)
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

    public void Save(RegionalVisitorBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalVisitorBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional visitor baselines. Empty fields match all.</summary>
public sealed record RegionalVisitorBaselineQuery(string Title, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalVisitorBaseline item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<RegionalVisitorBaseline> Apply(List<RegionalVisitorBaseline> items)
    {
        var matched = Collections.Filter(items, (RegionalVisitorBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional visitor baseline store.</summary>
public static class RegionalVisitorBaselineStore
{
    public static IRegionalVisitorBaselineStore NewMemoryRegionalVisitorBaselineStore()
    {
        return new MemoryRegionalVisitorBaselineStore(
            new Dictionary<string, RegionalVisitorBaseline>(),
            new List<string>());
    }

    public static IRegionalVisitorBaselineStore NewCachedRegionalVisitorBaselineStore(MemoryRegionalVisitorBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalVisitorBaselineStore(
            inner,
            RegionalVisitorBaselineCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RegionalVisitorBaseline> FindRegionalVisitorBaselines(IRegionalVisitorBaselineStore store, RegionalVisitorBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
