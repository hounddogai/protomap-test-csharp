namespace Acme.Workforce.Payrollops.Garnishment.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered garnishment targets by code.</summary>
public interface ITieredGarnishmentTargetStore
{
    TieredGarnishmentTarget? Find(string code);
    void Save(TieredGarnishmentTarget item);
    bool Remove(string code);
    List<TieredGarnishmentTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredGarnishmentTargetStore(Dictionary<string, TieredGarnishmentTarget> items, List<string> order) : ITieredGarnishmentTargetStore
{
    public Dictionary<string, TieredGarnishmentTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredGarnishmentTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredGarnishmentTarget item)
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

    public List<TieredGarnishmentTarget> List()
    {
        var outValue = new List<TieredGarnishmentTarget>();
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
public sealed class CachedTieredGarnishmentTargetStore(MemoryTieredGarnishmentTargetStore inner, TieredGarnishmentTargetCache cache, int hits, int misses) : ITieredGarnishmentTargetStore
{
    public MemoryTieredGarnishmentTargetStore Inner { get; set; } = inner;
    public TieredGarnishmentTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredGarnishmentTarget? Find(string code)
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

    public void Save(TieredGarnishmentTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredGarnishmentTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered garnishment targets. Empty fields match all.</summary>
public sealed record TieredGarnishmentTargetQuery(string Team, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(TieredGarnishmentTarget item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredGarnishmentTarget> Apply(List<TieredGarnishmentTarget> items)
    {
        var matched = Collections.Filter(items, (TieredGarnishmentTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered garnishment target store.</summary>
public static class TieredGarnishmentTargetStore
{
    public static ITieredGarnishmentTargetStore NewMemoryTieredGarnishmentTargetStore()
    {
        return new MemoryTieredGarnishmentTargetStore(
            new Dictionary<string, TieredGarnishmentTarget>(),
            new List<string>());
    }

    public static ITieredGarnishmentTargetStore NewCachedTieredGarnishmentTargetStore(MemoryTieredGarnishmentTargetStore inner, int ttl, IClock clock)
    {
        return new CachedTieredGarnishmentTargetStore(
            inner,
            TieredGarnishmentTargetCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<TieredGarnishmentTarget> FindTieredGarnishmentTargets(ITieredGarnishmentTargetStore store, TieredGarnishmentTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
