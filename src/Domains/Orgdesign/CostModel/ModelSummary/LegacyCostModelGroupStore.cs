namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy cost model groups by code.</summary>
public interface ILegacyCostModelGroupStore
{
    LegacyCostModelGroup? Find(string code);
    void Save(LegacyCostModelGroup item);
    bool Remove(string code);
    List<LegacyCostModelGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyCostModelGroupStore(Dictionary<string, LegacyCostModelGroup> items, List<string> order) : ILegacyCostModelGroupStore
{
    public Dictionary<string, LegacyCostModelGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyCostModelGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyCostModelGroup item)
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

    public List<LegacyCostModelGroup> List()
    {
        var outValue = new List<LegacyCostModelGroup>();
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
public sealed class CachedLegacyCostModelGroupStore(MemoryLegacyCostModelGroupStore inner, LegacyCostModelGroupCache cache, int hits, int misses) : ILegacyCostModelGroupStore
{
    public MemoryLegacyCostModelGroupStore Inner { get; set; } = inner;
    public LegacyCostModelGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyCostModelGroup? Find(string code)
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

    public void Save(LegacyCostModelGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyCostModelGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy cost model groups. Empty fields match all.</summary>
public sealed record LegacyCostModelGroupQuery(string Team, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyCostModelGroup item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<LegacyCostModelGroup> Apply(List<LegacyCostModelGroup> items)
    {
        var matched = Collections.Filter(items, (LegacyCostModelGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy cost model group store.</summary>
public static class LegacyCostModelGroupStore
{
    public static ILegacyCostModelGroupStore NewMemoryLegacyCostModelGroupStore()
    {
        return new MemoryLegacyCostModelGroupStore(
            new Dictionary<string, LegacyCostModelGroup>(),
            new List<string>());
    }

    public static ILegacyCostModelGroupStore NewCachedLegacyCostModelGroupStore(MemoryLegacyCostModelGroupStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyCostModelGroupStore(inner, LegacyCostModelGroupCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LegacyCostModelGroup> FindLegacyCostModelGroups(ILegacyCostModelGroupStore store, LegacyCostModelGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
