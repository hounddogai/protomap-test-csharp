namespace Acme.Workforce.Payrollops.Garnishment.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment preferences by code.</summary>
public interface IGarnishmentPreferenceStore
{
    GarnishmentPreference? Find(string code);
    void Save(GarnishmentPreference item);
    bool Remove(string code);
    List<GarnishmentPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentPreferenceStore(Dictionary<string, GarnishmentPreference> items, List<string> order) : IGarnishmentPreferenceStore
{
    public Dictionary<string, GarnishmentPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentPreference item)
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

    public List<GarnishmentPreference> List()
    {
        var outValue = new List<GarnishmentPreference>();
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
public sealed class CachedGarnishmentPreferenceStore(MemoryGarnishmentPreferenceStore inner, GarnishmentPreferenceCache cache, int hits, int misses) : IGarnishmentPreferenceStore
{
    public MemoryGarnishmentPreferenceStore Inner { get; set; } = inner;
    public GarnishmentPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentPreference? Find(string code)
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

    public void Save(GarnishmentPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment preferences. Empty fields match all.</summary>
public sealed record GarnishmentPreferenceQuery(string Title, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentPreference item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<GarnishmentPreference> Apply(List<GarnishmentPreference> items)
    {
        var matched = Collections.Filter(items, (GarnishmentPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment preference store.</summary>
public static class GarnishmentPreferenceStore
{
    public static IGarnishmentPreferenceStore NewMemoryGarnishmentPreferenceStore()
    {
        return new MemoryGarnishmentPreferenceStore(
            new Dictionary<string, GarnishmentPreference>(),
            new List<string>());
    }

    public static IGarnishmentPreferenceStore NewCachedGarnishmentPreferenceStore(MemoryGarnishmentPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentPreferenceStore(
            inner,
            GarnishmentPreferenceCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<GarnishmentPreference> FindGarnishmentPreferences(IGarnishmentPreferenceStore store, GarnishmentPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
