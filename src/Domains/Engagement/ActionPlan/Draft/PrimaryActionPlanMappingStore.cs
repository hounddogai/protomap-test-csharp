namespace Acme.Workforce.Engagement.ActionPlan.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary action plan mappings by code.</summary>
public interface IPrimaryActionPlanMappingStore
{
    PrimaryActionPlanMapping? Find(string code);
    void Save(PrimaryActionPlanMapping item);
    bool Remove(string code);
    List<PrimaryActionPlanMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryActionPlanMappingStore(Dictionary<string, PrimaryActionPlanMapping> items, List<string> order) : IPrimaryActionPlanMappingStore
{
    public Dictionary<string, PrimaryActionPlanMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryActionPlanMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryActionPlanMapping item)
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

    public List<PrimaryActionPlanMapping> List()
    {
        var outValue = new List<PrimaryActionPlanMapping>();
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
public sealed class CachedPrimaryActionPlanMappingStore(MemoryPrimaryActionPlanMappingStore inner, PrimaryActionPlanMappingCache cache, int hits, int misses) : IPrimaryActionPlanMappingStore
{
    public MemoryPrimaryActionPlanMappingStore Inner { get; set; } = inner;
    public PrimaryActionPlanMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryActionPlanMapping? Find(string code)
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

    public void Save(PrimaryActionPlanMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryActionPlanMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary action plan mappings. Empty fields match all.</summary>
public sealed record PrimaryActionPlanMappingQuery(string Segment, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryActionPlanMapping item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryActionPlanMapping> Apply(List<PrimaryActionPlanMapping> items)
    {
        var matched = Collections.Filter(items, (PrimaryActionPlanMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary action plan mapping store.</summary>
public static class PrimaryActionPlanMappingStore
{
    public static IPrimaryActionPlanMappingStore NewMemoryPrimaryActionPlanMappingStore()
    {
        return new MemoryPrimaryActionPlanMappingStore(
            new Dictionary<string, PrimaryActionPlanMapping>(),
            new List<string>());
    }

    public static IPrimaryActionPlanMappingStore NewCachedPrimaryActionPlanMappingStore(MemoryPrimaryActionPlanMappingStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryActionPlanMappingStore(
            inner,
            PrimaryActionPlanMappingCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryActionPlanMapping> FindPrimaryActionPlanMappings(IPrimaryActionPlanMappingStore store, PrimaryActionPlanMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
