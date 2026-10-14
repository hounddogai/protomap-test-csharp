namespace Acme.Workforce.Workplace.Desk.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk preferences by code.</summary>
public interface IDeskPreferenceStore
{
    DeskPreference? Find(string code);
    void Save(DeskPreference item);
    bool Remove(string code);
    List<DeskPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskPreferenceStore(Dictionary<string, DeskPreference> items, List<string> order) : IDeskPreferenceStore
{
    public Dictionary<string, DeskPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskPreference item)
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

    public List<DeskPreference> List()
    {
        var outValue = new List<DeskPreference>();
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
public sealed class CachedDeskPreferenceStore(MemoryDeskPreferenceStore inner, DeskPreferenceCache cache, int hits, int misses) : IDeskPreferenceStore
{
    public MemoryDeskPreferenceStore Inner { get; set; } = inner;
    public DeskPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskPreference? Find(string code)
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

    public void Save(DeskPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk preferences. Empty fields match all.</summary>
public sealed record DeskPreferenceQuery(string Label, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(DeskPreference item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskPreference> Apply(List<DeskPreference> items)
    {
        var matched = Collections.Filter(items, (DeskPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk preference store.</summary>
public static class DeskPreferenceStore
{
    public static IDeskPreferenceStore NewMemoryDeskPreferenceStore()
    {
        return new MemoryDeskPreferenceStore(new Dictionary<string, DeskPreference>(), new List<string>());
    }

    public static IDeskPreferenceStore NewCachedDeskPreferenceStore(MemoryDeskPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedDeskPreferenceStore(inner, DeskPreferenceCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<DeskPreference> FindDeskPreferences(IDeskPreferenceStore store, DeskPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
