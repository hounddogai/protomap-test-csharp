namespace Acme.Workforce.Talent.SkillProfile.ProfileEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback skill profile snapshots by code.</summary>
public interface IFallbackSkillProfileSnapshotStore
{
    FallbackSkillProfileSnapshot? Find(string code);
    void Save(FallbackSkillProfileSnapshot item);
    bool Remove(string code);
    List<FallbackSkillProfileSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackSkillProfileSnapshotStore(Dictionary<string, FallbackSkillProfileSnapshot> items, List<string> order) : IFallbackSkillProfileSnapshotStore
{
    public Dictionary<string, FallbackSkillProfileSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackSkillProfileSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackSkillProfileSnapshot item)
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

    public List<FallbackSkillProfileSnapshot> List()
    {
        var outValue = new List<FallbackSkillProfileSnapshot>();
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
public sealed class CachedFallbackSkillProfileSnapshotStore(MemoryFallbackSkillProfileSnapshotStore inner, FallbackSkillProfileSnapshotCache cache, int hits, int misses) : IFallbackSkillProfileSnapshotStore
{
    public MemoryFallbackSkillProfileSnapshotStore Inner { get; set; } = inner;
    public FallbackSkillProfileSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackSkillProfileSnapshot? Find(string code)
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

    public void Save(FallbackSkillProfileSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackSkillProfileSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback skill profile snapshots. Empty fields match all.</summary>
public sealed record FallbackSkillProfileSnapshotQuery(string Label, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackSkillProfileSnapshot item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<FallbackSkillProfileSnapshot> Apply(List<FallbackSkillProfileSnapshot> items)
    {
        var matched = Collections.Filter(items, (FallbackSkillProfileSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback skill profile snapshot store.</summary>
public static class FallbackSkillProfileSnapshotStore
{
    public static IFallbackSkillProfileSnapshotStore NewMemoryFallbackSkillProfileSnapshotStore()
    {
        return new MemoryFallbackSkillProfileSnapshotStore(
            new Dictionary<string, FallbackSkillProfileSnapshot>(),
            new List<string>());
    }

    public static IFallbackSkillProfileSnapshotStore NewCachedFallbackSkillProfileSnapshotStore(MemoryFallbackSkillProfileSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackSkillProfileSnapshotStore(
            inner,
            FallbackSkillProfileSnapshotCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<FallbackSkillProfileSnapshot> FindFallbackSkillProfileSnapshots(IFallbackSkillProfileSnapshotStore store, FallbackSkillProfileSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
