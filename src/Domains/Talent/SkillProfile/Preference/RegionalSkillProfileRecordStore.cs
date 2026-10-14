namespace Acme.Workforce.Talent.SkillProfile.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional skill profile records by code.</summary>
public interface IRegionalSkillProfileRecordStore
{
    RegionalSkillProfileRecord? Find(string code);
    void Save(RegionalSkillProfileRecord item);
    bool Remove(string code);
    List<RegionalSkillProfileRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalSkillProfileRecordStore(Dictionary<string, RegionalSkillProfileRecord> items, List<string> order) : IRegionalSkillProfileRecordStore
{
    public Dictionary<string, RegionalSkillProfileRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalSkillProfileRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalSkillProfileRecord item)
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

    public List<RegionalSkillProfileRecord> List()
    {
        var outValue = new List<RegionalSkillProfileRecord>();
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
public sealed class CachedRegionalSkillProfileRecordStore(MemoryRegionalSkillProfileRecordStore inner, RegionalSkillProfileRecordCache cache, int hits, int misses) : IRegionalSkillProfileRecordStore
{
    public MemoryRegionalSkillProfileRecordStore Inner { get; set; } = inner;
    public RegionalSkillProfileRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalSkillProfileRecord? Find(string code)
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

    public void Save(RegionalSkillProfileRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalSkillProfileRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional skill profile records. Empty fields match all.</summary>
public sealed record RegionalSkillProfileRecordQuery(string Team, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalSkillProfileRecord item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalSkillProfileRecord> Apply(List<RegionalSkillProfileRecord> items)
    {
        var matched = Collections.Filter(items, (RegionalSkillProfileRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional skill profile record store.</summary>
public static class RegionalSkillProfileRecordStore
{
    public static IRegionalSkillProfileRecordStore NewMemoryRegionalSkillProfileRecordStore()
    {
        return new MemoryRegionalSkillProfileRecordStore(
            new Dictionary<string, RegionalSkillProfileRecord>(),
            new List<string>());
    }

    public static IRegionalSkillProfileRecordStore NewCachedRegionalSkillProfileRecordStore(MemoryRegionalSkillProfileRecordStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalSkillProfileRecordStore(
            inner,
            RegionalSkillProfileRecordCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RegionalSkillProfileRecord> FindRegionalSkillProfileRecords(IRegionalSkillProfileRecordStore store, RegionalSkillProfileRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
