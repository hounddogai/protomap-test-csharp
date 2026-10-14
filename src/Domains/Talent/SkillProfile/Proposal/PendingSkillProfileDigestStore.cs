namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending skill profile digests by code.</summary>
public interface IPendingSkillProfileDigestStore
{
    PendingSkillProfileDigest? Find(string code);
    void Save(PendingSkillProfileDigest item);
    bool Remove(string code);
    List<PendingSkillProfileDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingSkillProfileDigestStore(Dictionary<string, PendingSkillProfileDigest> items, List<string> order) : IPendingSkillProfileDigestStore
{
    public Dictionary<string, PendingSkillProfileDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingSkillProfileDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingSkillProfileDigest item)
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

    public List<PendingSkillProfileDigest> List()
    {
        var outValue = new List<PendingSkillProfileDigest>();
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
public sealed class CachedPendingSkillProfileDigestStore(MemoryPendingSkillProfileDigestStore inner, PendingSkillProfileDigestCache cache, int hits, int misses) : IPendingSkillProfileDigestStore
{
    public MemoryPendingSkillProfileDigestStore Inner { get; set; } = inner;
    public PendingSkillProfileDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingSkillProfileDigest? Find(string code)
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

    public void Save(PendingSkillProfileDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingSkillProfileDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending skill profile digests. Empty fields match all.</summary>
public sealed record PendingSkillProfileDigestQuery(string Name, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(PendingSkillProfileDigest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingSkillProfileDigest> Apply(List<PendingSkillProfileDigest> items)
    {
        var matched = Collections.Filter(items, (PendingSkillProfileDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending skill profile digest store.</summary>
public static class PendingSkillProfileDigestStore
{
    public static IPendingSkillProfileDigestStore NewMemoryPendingSkillProfileDigestStore()
    {
        return new MemoryPendingSkillProfileDigestStore(
            new Dictionary<string, PendingSkillProfileDigest>(),
            new List<string>());
    }

    public static IPendingSkillProfileDigestStore NewCachedPendingSkillProfileDigestStore(MemoryPendingSkillProfileDigestStore inner, int ttl, IClock clock)
    {
        return new CachedPendingSkillProfileDigestStore(
            inner,
            PendingSkillProfileDigestCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PendingSkillProfileDigest> FindPendingSkillProfileDigests(IPendingSkillProfileDigestStore store, PendingSkillProfileDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
