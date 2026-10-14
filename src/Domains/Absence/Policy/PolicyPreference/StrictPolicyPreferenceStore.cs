namespace Acme.Workforce.Absence.Policy.PolicyPreference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict policy preferences by code.</summary>
public interface IStrictPolicyPreferenceStore
{
    StrictPolicyPreference? Find(string code);
    void Save(StrictPolicyPreference item);
    bool Remove(string code);
    List<StrictPolicyPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictPolicyPreferenceStore(Dictionary<string, StrictPolicyPreference> items, List<string> order) : IStrictPolicyPreferenceStore
{
    public Dictionary<string, StrictPolicyPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictPolicyPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictPolicyPreference item)
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

    public List<StrictPolicyPreference> List()
    {
        var outValue = new List<StrictPolicyPreference>();
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
public sealed class CachedStrictPolicyPreferenceStore(MemoryStrictPolicyPreferenceStore inner, StrictPolicyPreferenceCache cache, int hits, int misses) : IStrictPolicyPreferenceStore
{
    public MemoryStrictPolicyPreferenceStore Inner { get; set; } = inner;
    public StrictPolicyPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictPolicyPreference? Find(string code)
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

    public void Save(StrictPolicyPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictPolicyPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict policy preferences. Empty fields match all.</summary>
public sealed record StrictPolicyPreferenceQuery(string Channel, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(StrictPolicyPreference item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictPolicyPreference> Apply(List<StrictPolicyPreference> items)
    {
        var matched = Collections.Filter(items, (StrictPolicyPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict policy preference store.</summary>
public static class StrictPolicyPreferenceStore
{
    public static IStrictPolicyPreferenceStore NewMemoryStrictPolicyPreferenceStore()
    {
        return new MemoryStrictPolicyPreferenceStore(
            new Dictionary<string, StrictPolicyPreference>(),
            new List<string>());
    }

    public static IStrictPolicyPreferenceStore NewCachedStrictPolicyPreferenceStore(MemoryStrictPolicyPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedStrictPolicyPreferenceStore(
            inner,
            StrictPolicyPreferenceCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<StrictPolicyPreference> FindStrictPolicyPreferences(IStrictPolicyPreferenceStore store, StrictPolicyPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
