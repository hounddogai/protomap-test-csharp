namespace Acme.Workforce.Absence.Holiday.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday rules by code.</summary>
public interface IHolidayRuleStore
{
    HolidayRule? Find(string code);
    void Save(HolidayRule item);
    bool Remove(string code);
    List<HolidayRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayRuleStore(Dictionary<string, HolidayRule> items, List<string> order) : IHolidayRuleStore
{
    public Dictionary<string, HolidayRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayRule item)
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

    public List<HolidayRule> List()
    {
        var outValue = new List<HolidayRule>();
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
public sealed class CachedHolidayRuleStore(MemoryHolidayRuleStore inner, HolidayRuleCache cache, int hits, int misses) : IHolidayRuleStore
{
    public MemoryHolidayRuleStore Inner { get; set; } = inner;
    public HolidayRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayRule? Find(string code)
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

    public void Save(HolidayRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday rules. Empty fields match all.</summary>
public sealed record HolidayRuleQuery(string Title, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayRule item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidayRule> Apply(List<HolidayRule> items)
    {
        var matched = Collections.Filter(items, (HolidayRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday rule store.</summary>
public static class HolidayRuleStore
{
    public static IHolidayRuleStore NewMemoryHolidayRuleStore()
    {
        return new MemoryHolidayRuleStore(new Dictionary<string, HolidayRule>(), new List<string>());
    }

    public static IHolidayRuleStore NewCachedHolidayRuleStore(MemoryHolidayRuleStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayRuleStore(inner, HolidayRuleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<HolidayRule> FindHolidayRules(IHolidayRuleStore store, HolidayRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
