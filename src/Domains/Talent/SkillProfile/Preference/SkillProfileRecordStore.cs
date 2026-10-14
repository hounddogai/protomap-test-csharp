namespace Acme.Workforce.Talent.SkillProfile.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile records by code.</summary>
public interface ISkillProfileRecordStore
{
    SkillProfileRecord? Find(string code);
    void Save(SkillProfileRecord item);
    bool Remove(string code);
    List<SkillProfileRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfileRecordStore(Dictionary<string, SkillProfileRecord> items, List<string> order) : ISkillProfileRecordStore
{
    public Dictionary<string, SkillProfileRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfileRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfileRecord item)
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

    public List<SkillProfileRecord> List()
    {
        var outValue = new List<SkillProfileRecord>();
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
public sealed class CachedSkillProfileRecordStore(MemorySkillProfileRecordStore inner, SkillProfileRecordCache cache, int hits, int misses) : ISkillProfileRecordStore
{
    public MemorySkillProfileRecordStore Inner { get; set; } = inner;
    public SkillProfileRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfileRecord? Find(string code)
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

    public void Save(SkillProfileRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfileRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile records. Empty fields match all.</summary>
public sealed record SkillProfileRecordQuery(string Channel, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfileRecord item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SkillProfileRecord> Apply(List<SkillProfileRecord> items)
    {
        var matched = Collections.Filter(items, (SkillProfileRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile record store.</summary>
public static class SkillProfileRecordStore
{
    public static ISkillProfileRecordStore NewMemorySkillProfileRecordStore()
    {
        return new MemorySkillProfileRecordStore(new Dictionary<string, SkillProfileRecord>(), new List<string>());
    }

    public static ISkillProfileRecordStore NewCachedSkillProfileRecordStore(MemorySkillProfileRecordStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfileRecordStore(inner, SkillProfileRecordCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<SkillProfileRecord> FindSkillProfileRecords(ISkillProfileRecordStore store, SkillProfileRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
