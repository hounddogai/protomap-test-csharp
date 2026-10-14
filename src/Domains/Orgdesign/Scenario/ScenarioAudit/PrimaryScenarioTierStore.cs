namespace Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary scenario tiers by code.</summary>
public interface IPrimaryScenarioTierStore
{
    PrimaryScenarioTier? Find(string code);
    void Save(PrimaryScenarioTier item);
    bool Remove(string code);
    List<PrimaryScenarioTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryScenarioTierStore(Dictionary<string, PrimaryScenarioTier> items, List<string> order) : IPrimaryScenarioTierStore
{
    public Dictionary<string, PrimaryScenarioTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryScenarioTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryScenarioTier item)
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

    public List<PrimaryScenarioTier> List()
    {
        var outValue = new List<PrimaryScenarioTier>();
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
public sealed class CachedPrimaryScenarioTierStore(MemoryPrimaryScenarioTierStore inner, PrimaryScenarioTierCache cache, int hits, int misses) : IPrimaryScenarioTierStore
{
    public MemoryPrimaryScenarioTierStore Inner { get; set; } = inner;
    public PrimaryScenarioTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryScenarioTier? Find(string code)
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

    public void Save(PrimaryScenarioTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryScenarioTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary scenario tiers. Empty fields match all.</summary>
public sealed record PrimaryScenarioTierQuery(string Title, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryScenarioTier item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryScenarioTier> Apply(List<PrimaryScenarioTier> items)
    {
        var matched = Collections.Filter(items, (PrimaryScenarioTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary scenario tier store.</summary>
public static class PrimaryScenarioTierStore
{
    public static IPrimaryScenarioTierStore NewMemoryPrimaryScenarioTierStore()
    {
        return new MemoryPrimaryScenarioTierStore(new Dictionary<string, PrimaryScenarioTier>(), new List<string>());
    }

    public static IPrimaryScenarioTierStore NewCachedPrimaryScenarioTierStore(MemoryPrimaryScenarioTierStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryScenarioTierStore(inner, PrimaryScenarioTierCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PrimaryScenarioTier> FindPrimaryScenarioTiers(IPrimaryScenarioTierStore store, PrimaryScenarioTierQuery query)
    {
        return query.Apply(store.List());
    }
}
