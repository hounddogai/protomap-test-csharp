namespace Acme.Workforce.Talent.SkillProfile.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile entries by code.</summary>
public interface ISkillProfileEntryStore
{
    SkillProfileEntry? Find(string code);
    void Save(SkillProfileEntry item);
    bool Remove(string code);
    List<SkillProfileEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfileEntryStore(Dictionary<string, SkillProfileEntry> items, List<string> order) : ISkillProfileEntryStore
{
    public Dictionary<string, SkillProfileEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfileEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfileEntry item)
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

    public List<SkillProfileEntry> List()
    {
        var outValue = new List<SkillProfileEntry>();
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
public sealed class CachedSkillProfileEntryStore(MemorySkillProfileEntryStore inner, SkillProfileEntryCache cache, int hits, int misses) : ISkillProfileEntryStore
{
    public MemorySkillProfileEntryStore Inner { get; set; } = inner;
    public SkillProfileEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfileEntry? Find(string code)
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

    public void Save(SkillProfileEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfileEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile entries. Empty fields match all.</summary>
public sealed record SkillProfileEntryQuery(string Label, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfileEntry item)
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

    public List<SkillProfileEntry> Apply(List<SkillProfileEntry> items)
    {
        var matched = Collections.Filter(items, (SkillProfileEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile entry store.</summary>
public static class SkillProfileEntryStore
{
    public static ISkillProfileEntryStore NewMemorySkillProfileEntryStore()
    {
        return new MemorySkillProfileEntryStore(new Dictionary<string, SkillProfileEntry>(), new List<string>());
    }

    public static ISkillProfileEntryStore NewCachedSkillProfileEntryStore(MemorySkillProfileEntryStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfileEntryStore(inner, SkillProfileEntryCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SkillProfileEntry> FindSkillProfileEntries(ISkillProfileEntryStore store, SkillProfileEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
