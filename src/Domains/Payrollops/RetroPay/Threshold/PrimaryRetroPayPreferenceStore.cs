namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary retro pay preferences by code.</summary>
public interface IPrimaryRetroPayPreferenceStore
{
    PrimaryRetroPayPreference? Find(string code);
    void Save(PrimaryRetroPayPreference item);
    bool Remove(string code);
    List<PrimaryRetroPayPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryRetroPayPreferenceStore(Dictionary<string, PrimaryRetroPayPreference> items, List<string> order) : IPrimaryRetroPayPreferenceStore
{
    public Dictionary<string, PrimaryRetroPayPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryRetroPayPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryRetroPayPreference item)
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

    public List<PrimaryRetroPayPreference> List()
    {
        var outValue = new List<PrimaryRetroPayPreference>();
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
public sealed class CachedPrimaryRetroPayPreferenceStore(MemoryPrimaryRetroPayPreferenceStore inner, PrimaryRetroPayPreferenceCache cache, int hits, int misses) : IPrimaryRetroPayPreferenceStore
{
    public MemoryPrimaryRetroPayPreferenceStore Inner { get; set; } = inner;
    public PrimaryRetroPayPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryRetroPayPreference? Find(string code)
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

    public void Save(PrimaryRetroPayPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryRetroPayPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary retro pay preferences. Empty fields match all.</summary>
public sealed record PrimaryRetroPayPreferenceQuery(string Name, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryRetroPayPreference item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryRetroPayPreference> Apply(List<PrimaryRetroPayPreference> items)
    {
        var matched = Collections.Filter(items, (PrimaryRetroPayPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary retro pay preference store.</summary>
public static class PrimaryRetroPayPreferenceStore
{
    public static IPrimaryRetroPayPreferenceStore NewMemoryPrimaryRetroPayPreferenceStore()
    {
        return new MemoryPrimaryRetroPayPreferenceStore(
            new Dictionary<string, PrimaryRetroPayPreference>(),
            new List<string>());
    }

    public static IPrimaryRetroPayPreferenceStore NewCachedPrimaryRetroPayPreferenceStore(MemoryPrimaryRetroPayPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryRetroPayPreferenceStore(
            inner,
            PrimaryRetroPayPreferenceCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryRetroPayPreference> FindPrimaryRetroPayPreferences(IPrimaryRetroPayPreferenceStore store, PrimaryRetroPayPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
