namespace Acme.Workforce.Orgdesign.Reorg.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg profiles by code.</summary>
public interface IReorgProfileStore
{
    ReorgProfile? Find(string code);
    void Save(ReorgProfile item);
    bool Remove(string code);
    List<ReorgProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgProfileStore(Dictionary<string, ReorgProfile> items, List<string> order) : IReorgProfileStore
{
    public Dictionary<string, ReorgProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgProfile item)
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

    public List<ReorgProfile> List()
    {
        var outValue = new List<ReorgProfile>();
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
public sealed class CachedReorgProfileStore(MemoryReorgProfileStore inner, ReorgProfileCache cache, int hits, int misses) : IReorgProfileStore
{
    public MemoryReorgProfileStore Inner { get; set; } = inner;
    public ReorgProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgProfile? Find(string code)
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

    public void Save(ReorgProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg profiles. Empty fields match all.</summary>
public sealed record ReorgProfileQuery(string Label, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgProfile item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReorgProfile> Apply(List<ReorgProfile> items)
    {
        var matched = Collections.Filter(items, (ReorgProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg profile store.</summary>
public static class ReorgProfileStore
{
    public static IReorgProfileStore NewMemoryReorgProfileStore()
    {
        return new MemoryReorgProfileStore(new Dictionary<string, ReorgProfile>(), new List<string>());
    }

    public static IReorgProfileStore NewCachedReorgProfileStore(MemoryReorgProfileStore inner, int ttl, IClock clock)
    {
        return new CachedReorgProfileStore(inner, ReorgProfileCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ReorgProfile> FindReorgProfiles(IReorgProfileStore store, ReorgProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
