namespace Acme.Workforce.Payrollops.Garnishment.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment targets by code.</summary>
public interface IGarnishmentTargetStore
{
    GarnishmentTarget? Find(string code);
    void Save(GarnishmentTarget item);
    bool Remove(string code);
    List<GarnishmentTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentTargetStore(Dictionary<string, GarnishmentTarget> items, List<string> order) : IGarnishmentTargetStore
{
    public Dictionary<string, GarnishmentTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentTarget item)
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

    public List<GarnishmentTarget> List()
    {
        var outValue = new List<GarnishmentTarget>();
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
public sealed class CachedGarnishmentTargetStore(MemoryGarnishmentTargetStore inner, GarnishmentTargetCache cache, int hits, int misses) : IGarnishmentTargetStore
{
    public MemoryGarnishmentTargetStore Inner { get; set; } = inner;
    public GarnishmentTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentTarget? Find(string code)
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

    public void Save(GarnishmentTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment targets. Empty fields match all.</summary>
public sealed record GarnishmentTargetQuery(string Country, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentTarget item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GarnishmentTarget> Apply(List<GarnishmentTarget> items)
    {
        var matched = Collections.Filter(items, (GarnishmentTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment target store.</summary>
public static class GarnishmentTargetStore
{
    public static IGarnishmentTargetStore NewMemoryGarnishmentTargetStore()
    {
        return new MemoryGarnishmentTargetStore(new Dictionary<string, GarnishmentTarget>(), new List<string>());
    }

    public static IGarnishmentTargetStore NewCachedGarnishmentTargetStore(MemoryGarnishmentTargetStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentTargetStore(inner, GarnishmentTargetCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<GarnishmentTarget> FindGarnishmentTargets(IGarnishmentTargetStore store, GarnishmentTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
