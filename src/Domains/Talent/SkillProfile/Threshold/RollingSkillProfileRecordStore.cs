namespace Acme.Workforce.Talent.SkillProfile.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling skill profile records by code.</summary>
public interface IRollingSkillProfileRecordStore
{
    RollingSkillProfileRecord? Find(string code);
    void Save(RollingSkillProfileRecord item);
    bool Remove(string code);
    List<RollingSkillProfileRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingSkillProfileRecordStore(Dictionary<string, RollingSkillProfileRecord> items, List<string> order) : IRollingSkillProfileRecordStore
{
    public Dictionary<string, RollingSkillProfileRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingSkillProfileRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingSkillProfileRecord item)
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

    public List<RollingSkillProfileRecord> List()
    {
        var outValue = new List<RollingSkillProfileRecord>();
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
public sealed class CachedRollingSkillProfileRecordStore(MemoryRollingSkillProfileRecordStore inner, RollingSkillProfileRecordCache cache, int hits, int misses) : IRollingSkillProfileRecordStore
{
    public MemoryRollingSkillProfileRecordStore Inner { get; set; } = inner;
    public RollingSkillProfileRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingSkillProfileRecord? Find(string code)
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

    public void Save(RollingSkillProfileRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingSkillProfileRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling skill profile records. Empty fields match all.</summary>
public sealed record RollingSkillProfileRecordQuery(string Category, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(RollingSkillProfileRecord item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingSkillProfileRecord> Apply(List<RollingSkillProfileRecord> items)
    {
        var matched = Collections.Filter(items, (RollingSkillProfileRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling skill profile record store.</summary>
public static class RollingSkillProfileRecordStore
{
    public static IRollingSkillProfileRecordStore NewMemoryRollingSkillProfileRecordStore()
    {
        return new MemoryRollingSkillProfileRecordStore(
            new Dictionary<string, RollingSkillProfileRecord>(),
            new List<string>());
    }

    public static IRollingSkillProfileRecordStore NewCachedRollingSkillProfileRecordStore(MemoryRollingSkillProfileRecordStore inner, int ttl, IClock clock)
    {
        return new CachedRollingSkillProfileRecordStore(
            inner,
            RollingSkillProfileRecordCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<RollingSkillProfileRecord> FindRollingSkillProfileRecords(IRollingSkillProfileRecordStore store, RollingSkillProfileRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
