namespace Acme.Workforce.Orgdesign.Headcount.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default headcount entries by code.</summary>
public interface IDefaultHeadcountEntryStore
{
    DefaultHeadcountEntry? Find(string code);
    void Save(DefaultHeadcountEntry item);
    bool Remove(string code);
    List<DefaultHeadcountEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultHeadcountEntryStore(Dictionary<string, DefaultHeadcountEntry> items, List<string> order) : IDefaultHeadcountEntryStore
{
    public Dictionary<string, DefaultHeadcountEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultHeadcountEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultHeadcountEntry item)
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

    public List<DefaultHeadcountEntry> List()
    {
        var outValue = new List<DefaultHeadcountEntry>();
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
public sealed class CachedDefaultHeadcountEntryStore(MemoryDefaultHeadcountEntryStore inner, DefaultHeadcountEntryCache cache, int hits, int misses) : IDefaultHeadcountEntryStore
{
    public MemoryDefaultHeadcountEntryStore Inner { get; set; } = inner;
    public DefaultHeadcountEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultHeadcountEntry? Find(string code)
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

    public void Save(DefaultHeadcountEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultHeadcountEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default headcount entries. Empty fields match all.</summary>
public sealed record DefaultHeadcountEntryQuery(string Label, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultHeadcountEntry item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultHeadcountEntry> Apply(List<DefaultHeadcountEntry> items)
    {
        var matched = Collections.Filter(items, (DefaultHeadcountEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default headcount entry store.</summary>
public static class DefaultHeadcountEntryStore
{
    public static IDefaultHeadcountEntryStore NewMemoryDefaultHeadcountEntryStore()
    {
        return new MemoryDefaultHeadcountEntryStore(
            new Dictionary<string, DefaultHeadcountEntry>(),
            new List<string>());
    }

    public static IDefaultHeadcountEntryStore NewCachedDefaultHeadcountEntryStore(MemoryDefaultHeadcountEntryStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultHeadcountEntryStore(inner, DefaultHeadcountEntryCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DefaultHeadcountEntry> FindDefaultHeadcountEntries(IDefaultHeadcountEntryStore store, DefaultHeadcountEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
