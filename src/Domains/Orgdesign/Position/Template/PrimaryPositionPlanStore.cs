namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary position plans by code.</summary>
public interface IPrimaryPositionPlanStore
{
    PrimaryPositionPlan? Find(string code);
    void Save(PrimaryPositionPlan item);
    bool Remove(string code);
    List<PrimaryPositionPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryPositionPlanStore(Dictionary<string, PrimaryPositionPlan> items, List<string> order) : IPrimaryPositionPlanStore
{
    public Dictionary<string, PrimaryPositionPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryPositionPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryPositionPlan item)
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

    public List<PrimaryPositionPlan> List()
    {
        var outValue = new List<PrimaryPositionPlan>();
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
public sealed class CachedPrimaryPositionPlanStore(MemoryPrimaryPositionPlanStore inner, PrimaryPositionPlanCache cache, int hits, int misses) : IPrimaryPositionPlanStore
{
    public MemoryPrimaryPositionPlanStore Inner { get; set; } = inner;
    public PrimaryPositionPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryPositionPlan? Find(string code)
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

    public void Save(PrimaryPositionPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryPositionPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary position plans. Empty fields match all.</summary>
public sealed record PrimaryPositionPlanQuery(string Country, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryPositionPlan item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<PrimaryPositionPlan> Apply(List<PrimaryPositionPlan> items)
    {
        var matched = Collections.Filter(items, (PrimaryPositionPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary position plan store.</summary>
public static class PrimaryPositionPlanStore
{
    public static IPrimaryPositionPlanStore NewMemoryPrimaryPositionPlanStore()
    {
        return new MemoryPrimaryPositionPlanStore(new Dictionary<string, PrimaryPositionPlan>(), new List<string>());
    }

    public static IPrimaryPositionPlanStore NewCachedPrimaryPositionPlanStore(MemoryPrimaryPositionPlanStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryPositionPlanStore(inner, PrimaryPositionPlanCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PrimaryPositionPlan> FindPrimaryPositionPlans(IPrimaryPositionPlanStore store, PrimaryPositionPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
