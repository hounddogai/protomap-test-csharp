namespace Acme.Workforce.Assets.LicenseSeat.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered license seat rules by code.</summary>
public interface ITieredLicenseSeatRuleStore
{
    TieredLicenseSeatRule? Find(string code);
    void Save(TieredLicenseSeatRule item);
    bool Remove(string code);
    List<TieredLicenseSeatRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredLicenseSeatRuleStore(Dictionary<string, TieredLicenseSeatRule> items, List<string> order) : ITieredLicenseSeatRuleStore
{
    public Dictionary<string, TieredLicenseSeatRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredLicenseSeatRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredLicenseSeatRule item)
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

    public List<TieredLicenseSeatRule> List()
    {
        var outValue = new List<TieredLicenseSeatRule>();
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
public sealed class CachedTieredLicenseSeatRuleStore(MemoryTieredLicenseSeatRuleStore inner, TieredLicenseSeatRuleCache cache, int hits, int misses) : ITieredLicenseSeatRuleStore
{
    public MemoryTieredLicenseSeatRuleStore Inner { get; set; } = inner;
    public TieredLicenseSeatRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredLicenseSeatRule? Find(string code)
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

    public void Save(TieredLicenseSeatRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredLicenseSeatRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered license seat rules. Empty fields match all.</summary>
public sealed record TieredLicenseSeatRuleQuery(string Label, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(TieredLicenseSeatRule item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredLicenseSeatRule> Apply(List<TieredLicenseSeatRule> items)
    {
        var matched = Collections.Filter(items, (TieredLicenseSeatRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered license seat rule store.</summary>
public static class TieredLicenseSeatRuleStore
{
    public static ITieredLicenseSeatRuleStore NewMemoryTieredLicenseSeatRuleStore()
    {
        return new MemoryTieredLicenseSeatRuleStore(
            new Dictionary<string, TieredLicenseSeatRule>(),
            new List<string>());
    }

    public static ITieredLicenseSeatRuleStore NewCachedTieredLicenseSeatRuleStore(MemoryTieredLicenseSeatRuleStore inner, int ttl, IClock clock)
    {
        return new CachedTieredLicenseSeatRuleStore(inner, TieredLicenseSeatRuleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TieredLicenseSeatRule> FindTieredLicenseSeatRules(ITieredLicenseSeatRuleStore store, TieredLicenseSeatRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
