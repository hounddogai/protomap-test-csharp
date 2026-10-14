namespace Acme.Workforce.Workplace.Desk.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk rules by code.</summary>
public interface IDeskRuleStore
{
    DeskRule? Find(string code);
    void Save(DeskRule item);
    bool Remove(string code);
    List<DeskRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskRuleStore(Dictionary<string, DeskRule> items, List<string> order) : IDeskRuleStore
{
    public Dictionary<string, DeskRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskRule item)
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

    public List<DeskRule> List()
    {
        var outValue = new List<DeskRule>();
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
public sealed class CachedDeskRuleStore(MemoryDeskRuleStore inner, DeskRuleCache cache, int hits, int misses) : IDeskRuleStore
{
    public MemoryDeskRuleStore Inner { get; set; } = inner;
    public DeskRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskRule? Find(string code)
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

    public void Save(DeskRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk rules. Empty fields match all.</summary>
public sealed record DeskRuleQuery(string Source, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(DeskRule item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskRule> Apply(List<DeskRule> items)
    {
        var matched = Collections.Filter(items, (DeskRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk rule store.</summary>
public static class DeskRuleStore
{
    public static IDeskRuleStore NewMemoryDeskRuleStore()
    {
        return new MemoryDeskRuleStore(new Dictionary<string, DeskRule>(), new List<string>());
    }

    public static IDeskRuleStore NewCachedDeskRuleStore(MemoryDeskRuleStore inner, int ttl, IClock clock)
    {
        return new CachedDeskRuleStore(inner, DeskRuleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DeskRule> FindDeskRules(IDeskRuleStore store, DeskRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
