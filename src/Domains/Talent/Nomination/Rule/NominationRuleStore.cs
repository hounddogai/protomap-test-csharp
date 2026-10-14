namespace Acme.Workforce.Talent.Nomination.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination rules by code.</summary>
public interface INominationRuleStore
{
    NominationRule? Find(string code);
    void Save(NominationRule item);
    bool Remove(string code);
    List<NominationRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationRuleStore(Dictionary<string, NominationRule> items, List<string> order) : INominationRuleStore
{
    public Dictionary<string, NominationRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationRule item)
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

    public List<NominationRule> List()
    {
        var outValue = new List<NominationRule>();
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
public sealed class CachedNominationRuleStore(MemoryNominationRuleStore inner, NominationRuleCache cache, int hits, int misses) : INominationRuleStore
{
    public MemoryNominationRuleStore Inner { get; set; } = inner;
    public NominationRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationRule? Find(string code)
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

    public void Save(NominationRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination rules. Empty fields match all.</summary>
public sealed record NominationRuleQuery(string Channel, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(NominationRule item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<NominationRule> Apply(List<NominationRule> items)
    {
        var matched = Collections.Filter(items, (NominationRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination rule store.</summary>
public static class NominationRuleStore
{
    public static INominationRuleStore NewMemoryNominationRuleStore()
    {
        return new MemoryNominationRuleStore(new Dictionary<string, NominationRule>(), new List<string>());
    }

    public static INominationRuleStore NewCachedNominationRuleStore(MemoryNominationRuleStore inner, int ttl, IClock clock)
    {
        return new CachedNominationRuleStore(inner, NominationRuleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<NominationRule> FindNominationRules(INominationRuleStore store, NominationRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
