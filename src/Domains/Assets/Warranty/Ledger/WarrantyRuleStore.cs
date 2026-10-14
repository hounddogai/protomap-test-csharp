namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty rules by code.</summary>
public interface IWarrantyRuleStore
{
    WarrantyRule? Find(string code);
    void Save(WarrantyRule item);
    bool Remove(string code);
    List<WarrantyRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyRuleStore(Dictionary<string, WarrantyRule> items, List<string> order) : IWarrantyRuleStore
{
    public Dictionary<string, WarrantyRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyRule item)
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

    public List<WarrantyRule> List()
    {
        var outValue = new List<WarrantyRule>();
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
public sealed class CachedWarrantyRuleStore(MemoryWarrantyRuleStore inner, WarrantyRuleCache cache, int hits, int misses) : IWarrantyRuleStore
{
    public MemoryWarrantyRuleStore Inner { get; set; } = inner;
    public WarrantyRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyRule? Find(string code)
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

    public void Save(WarrantyRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty rules. Empty fields match all.</summary>
public sealed record WarrantyRuleQuery(string Label, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyRule item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<WarrantyRule> Apply(List<WarrantyRule> items)
    {
        var matched = Collections.Filter(items, (WarrantyRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty rule store.</summary>
public static class WarrantyRuleStore
{
    public static IWarrantyRuleStore NewMemoryWarrantyRuleStore()
    {
        return new MemoryWarrantyRuleStore(new Dictionary<string, WarrantyRule>(), new List<string>());
    }

    public static IWarrantyRuleStore NewCachedWarrantyRuleStore(MemoryWarrantyRuleStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyRuleStore(inner, WarrantyRuleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<WarrantyRule> FindWarrantyRules(IWarrantyRuleStore store, WarrantyRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
