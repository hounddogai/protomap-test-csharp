namespace Acme.Workforce.Talent.SkillProfile.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy skill profile checkpoints by code.</summary>
public interface ILegacySkillProfileCheckpointStore
{
    LegacySkillProfileCheckpoint? Find(string code);
    void Save(LegacySkillProfileCheckpoint item);
    bool Remove(string code);
    List<LegacySkillProfileCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacySkillProfileCheckpointStore(Dictionary<string, LegacySkillProfileCheckpoint> items, List<string> order) : ILegacySkillProfileCheckpointStore
{
    public Dictionary<string, LegacySkillProfileCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacySkillProfileCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacySkillProfileCheckpoint item)
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

    public List<LegacySkillProfileCheckpoint> List()
    {
        var outValue = new List<LegacySkillProfileCheckpoint>();
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
public sealed class CachedLegacySkillProfileCheckpointStore(MemoryLegacySkillProfileCheckpointStore inner, LegacySkillProfileCheckpointCache cache, int hits, int misses) : ILegacySkillProfileCheckpointStore
{
    public MemoryLegacySkillProfileCheckpointStore Inner { get; set; } = inner;
    public LegacySkillProfileCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacySkillProfileCheckpoint? Find(string code)
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

    public void Save(LegacySkillProfileCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacySkillProfileCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy skill profile checkpoints. Empty fields match all.</summary>
public sealed record LegacySkillProfileCheckpointQuery(string Name, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(LegacySkillProfileCheckpoint item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacySkillProfileCheckpoint> Apply(List<LegacySkillProfileCheckpoint> items)
    {
        var matched = Collections.Filter(items, (LegacySkillProfileCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy skill profile checkpoint store.</summary>
public static class LegacySkillProfileCheckpointStore
{
    public static ILegacySkillProfileCheckpointStore NewMemoryLegacySkillProfileCheckpointStore()
    {
        return new MemoryLegacySkillProfileCheckpointStore(
            new Dictionary<string, LegacySkillProfileCheckpoint>(),
            new List<string>());
    }

    public static ILegacySkillProfileCheckpointStore NewCachedLegacySkillProfileCheckpointStore(MemoryLegacySkillProfileCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedLegacySkillProfileCheckpointStore(
            inner,
            LegacySkillProfileCheckpointCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LegacySkillProfileCheckpoint> FindLegacySkillProfileCheckpoints(ILegacySkillProfileCheckpointStore store, LegacySkillProfileCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
