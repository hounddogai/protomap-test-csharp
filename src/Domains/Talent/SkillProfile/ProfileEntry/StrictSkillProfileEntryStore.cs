namespace Acme.Workforce.Talent.SkillProfile.ProfileEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict skill profile entries by code.</summary>
public interface IStrictSkillProfileEntryStore
{
    StrictSkillProfileEntry? Find(string code);
    void Save(StrictSkillProfileEntry item);
    bool Remove(string code);
    List<StrictSkillProfileEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictSkillProfileEntryStore(Dictionary<string, StrictSkillProfileEntry> items, List<string> order) : IStrictSkillProfileEntryStore
{
    public Dictionary<string, StrictSkillProfileEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictSkillProfileEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictSkillProfileEntry item)
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

    public List<StrictSkillProfileEntry> List()
    {
        var outValue = new List<StrictSkillProfileEntry>();
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
public sealed class CachedStrictSkillProfileEntryStore(MemoryStrictSkillProfileEntryStore inner, StrictSkillProfileEntryCache cache, int hits, int misses) : IStrictSkillProfileEntryStore
{
    public MemoryStrictSkillProfileEntryStore Inner { get; set; } = inner;
    public StrictSkillProfileEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictSkillProfileEntry? Find(string code)
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

    public void Save(StrictSkillProfileEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictSkillProfileEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict skill profile entries. Empty fields match all.</summary>
public sealed record StrictSkillProfileEntryQuery(string Team, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(StrictSkillProfileEntry item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictSkillProfileEntry> Apply(List<StrictSkillProfileEntry> items)
    {
        var matched = Collections.Filter(items, (StrictSkillProfileEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict skill profile entry store.</summary>
public static class StrictSkillProfileEntryStore
{
    public static IStrictSkillProfileEntryStore NewMemoryStrictSkillProfileEntryStore()
    {
        return new MemoryStrictSkillProfileEntryStore(
            new Dictionary<string, StrictSkillProfileEntry>(),
            new List<string>());
    }

    public static IStrictSkillProfileEntryStore NewCachedStrictSkillProfileEntryStore(MemoryStrictSkillProfileEntryStore inner, int ttl, IClock clock)
    {
        return new CachedStrictSkillProfileEntryStore(
            inner,
            StrictSkillProfileEntryCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<StrictSkillProfileEntry> FindStrictSkillProfileEntries(IStrictSkillProfileEntryStore store, StrictSkillProfileEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
