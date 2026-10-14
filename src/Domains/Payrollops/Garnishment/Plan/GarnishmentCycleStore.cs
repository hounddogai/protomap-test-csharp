namespace Acme.Workforce.Payrollops.Garnishment.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment cycles by code.</summary>
public interface IGarnishmentCycleStore
{
    GarnishmentCycle? Find(string code);
    void Save(GarnishmentCycle item);
    bool Remove(string code);
    List<GarnishmentCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentCycleStore(Dictionary<string, GarnishmentCycle> items, List<string> order) : IGarnishmentCycleStore
{
    public Dictionary<string, GarnishmentCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentCycle item)
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

    public List<GarnishmentCycle> List()
    {
        var outValue = new List<GarnishmentCycle>();
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
public sealed class CachedGarnishmentCycleStore(MemoryGarnishmentCycleStore inner, GarnishmentCycleCache cache, int hits, int misses) : IGarnishmentCycleStore
{
    public MemoryGarnishmentCycleStore Inner { get; set; } = inner;
    public GarnishmentCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentCycle? Find(string code)
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

    public void Save(GarnishmentCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment cycles. Empty fields match all.</summary>
public sealed record GarnishmentCycleQuery(string Unit, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentCycle item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GarnishmentCycle> Apply(List<GarnishmentCycle> items)
    {
        var matched = Collections.Filter(items, (GarnishmentCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment cycle store.</summary>
public static class GarnishmentCycleStore
{
    public static IGarnishmentCycleStore NewMemoryGarnishmentCycleStore()
    {
        return new MemoryGarnishmentCycleStore(new Dictionary<string, GarnishmentCycle>(), new List<string>());
    }

    public static IGarnishmentCycleStore NewCachedGarnishmentCycleStore(MemoryGarnishmentCycleStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentCycleStore(inner, GarnishmentCycleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<GarnishmentCycle> FindGarnishmentCycles(IGarnishmentCycleStore store, GarnishmentCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
