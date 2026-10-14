namespace Acme.Workforce.Compliance.PolicyAck.AckGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict policy ack templates by code.</summary>
public interface IStrictPolicyAckTemplateStore
{
    StrictPolicyAckTemplate? Find(string code);
    void Save(StrictPolicyAckTemplate item);
    bool Remove(string code);
    List<StrictPolicyAckTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictPolicyAckTemplateStore(Dictionary<string, StrictPolicyAckTemplate> items, List<string> order) : IStrictPolicyAckTemplateStore
{
    public Dictionary<string, StrictPolicyAckTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictPolicyAckTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictPolicyAckTemplate item)
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

    public List<StrictPolicyAckTemplate> List()
    {
        var outValue = new List<StrictPolicyAckTemplate>();
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
public sealed class CachedStrictPolicyAckTemplateStore(MemoryStrictPolicyAckTemplateStore inner, StrictPolicyAckTemplateCache cache, int hits, int misses) : IStrictPolicyAckTemplateStore
{
    public MemoryStrictPolicyAckTemplateStore Inner { get; set; } = inner;
    public StrictPolicyAckTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictPolicyAckTemplate? Find(string code)
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

    public void Save(StrictPolicyAckTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictPolicyAckTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict policy ack templates. Empty fields match all.</summary>
public sealed record StrictPolicyAckTemplateQuery(string Unit, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(StrictPolicyAckTemplate item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictPolicyAckTemplate> Apply(List<StrictPolicyAckTemplate> items)
    {
        var matched = Collections.Filter(items, (StrictPolicyAckTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict policy ack template store.</summary>
public static class StrictPolicyAckTemplateStore
{
    public static IStrictPolicyAckTemplateStore NewMemoryStrictPolicyAckTemplateStore()
    {
        return new MemoryStrictPolicyAckTemplateStore(
            new Dictionary<string, StrictPolicyAckTemplate>(),
            new List<string>());
    }

    public static IStrictPolicyAckTemplateStore NewCachedStrictPolicyAckTemplateStore(MemoryStrictPolicyAckTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedStrictPolicyAckTemplateStore(
            inner,
            StrictPolicyAckTemplateCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StrictPolicyAckTemplate> FindStrictPolicyAckTemplates(IStrictPolicyAckTemplateStore store, StrictPolicyAckTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
