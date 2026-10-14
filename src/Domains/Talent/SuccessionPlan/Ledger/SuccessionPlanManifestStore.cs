namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan manifests by code.</summary>
public interface ISuccessionPlanManifestStore
{
    SuccessionPlanManifest? Find(string code);
    void Save(SuccessionPlanManifest item);
    bool Remove(string code);
    List<SuccessionPlanManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanManifestStore(Dictionary<string, SuccessionPlanManifest> items, List<string> order) : ISuccessionPlanManifestStore
{
    public Dictionary<string, SuccessionPlanManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanManifest item)
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

    public List<SuccessionPlanManifest> List()
    {
        var outValue = new List<SuccessionPlanManifest>();
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
public sealed class CachedSuccessionPlanManifestStore(MemorySuccessionPlanManifestStore inner, SuccessionPlanManifestCache cache, int hits, int misses) : ISuccessionPlanManifestStore
{
    public MemorySuccessionPlanManifestStore Inner { get; set; } = inner;
    public SuccessionPlanManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanManifest? Find(string code)
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

    public void Save(SuccessionPlanManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan manifests. Empty fields match all.</summary>
public sealed record SuccessionPlanManifestQuery(string Segment, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanManifest item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SuccessionPlanManifest> Apply(List<SuccessionPlanManifest> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan manifest store.</summary>
public static class SuccessionPlanManifestStore
{
    public static ISuccessionPlanManifestStore NewMemorySuccessionPlanManifestStore()
    {
        return new MemorySuccessionPlanManifestStore(
            new Dictionary<string, SuccessionPlanManifest>(),
            new List<string>());
    }

    public static ISuccessionPlanManifestStore NewCachedSuccessionPlanManifestStore(MemorySuccessionPlanManifestStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanManifestStore(
            inner,
            SuccessionPlanManifestCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<SuccessionPlanManifest> FindSuccessionPlanManifests(ISuccessionPlanManifestStore store, SuccessionPlanManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
