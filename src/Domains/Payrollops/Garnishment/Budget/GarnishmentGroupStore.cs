namespace Acme.Workforce.Payrollops.Garnishment.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment groups by code.</summary>
public interface IGarnishmentGroupStore
{
    GarnishmentGroup? Find(string code);
    void Save(GarnishmentGroup item);
    bool Remove(string code);
    List<GarnishmentGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentGroupStore(Dictionary<string, GarnishmentGroup> items, List<string> order) : IGarnishmentGroupStore
{
    public Dictionary<string, GarnishmentGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentGroup item)
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

    public List<GarnishmentGroup> List()
    {
        var outValue = new List<GarnishmentGroup>();
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
public sealed class CachedGarnishmentGroupStore(MemoryGarnishmentGroupStore inner, GarnishmentGroupCache cache, int hits, int misses) : IGarnishmentGroupStore
{
    public MemoryGarnishmentGroupStore Inner { get; set; } = inner;
    public GarnishmentGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentGroup? Find(string code)
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

    public void Save(GarnishmentGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment groups. Empty fields match all.</summary>
public sealed record GarnishmentGroupQuery(string Unit, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentGroup item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<GarnishmentGroup> Apply(List<GarnishmentGroup> items)
    {
        var matched = Collections.Filter(items, (GarnishmentGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment group store.</summary>
public static class GarnishmentGroupStore
{
    public static IGarnishmentGroupStore NewMemoryGarnishmentGroupStore()
    {
        return new MemoryGarnishmentGroupStore(new Dictionary<string, GarnishmentGroup>(), new List<string>());
    }

    public static IGarnishmentGroupStore NewCachedGarnishmentGroupStore(MemoryGarnishmentGroupStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentGroupStore(inner, GarnishmentGroupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<GarnishmentGroup> FindGarnishmentGroups(IGarnishmentGroupStore store, GarnishmentGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
