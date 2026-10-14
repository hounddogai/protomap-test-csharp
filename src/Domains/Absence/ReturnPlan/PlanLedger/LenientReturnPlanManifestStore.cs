namespace Acme.Workforce.Absence.ReturnPlan.PlanLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient return plan manifests by code.</summary>
public interface ILenientReturnPlanManifestStore
{
    LenientReturnPlanManifest? Find(string code);
    void Save(LenientReturnPlanManifest item);
    bool Remove(string code);
    List<LenientReturnPlanManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientReturnPlanManifestStore(Dictionary<string, LenientReturnPlanManifest> items, List<string> order) : ILenientReturnPlanManifestStore
{
    public Dictionary<string, LenientReturnPlanManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientReturnPlanManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientReturnPlanManifest item)
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

    public List<LenientReturnPlanManifest> List()
    {
        var outValue = new List<LenientReturnPlanManifest>();
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
public sealed class CachedLenientReturnPlanManifestStore(MemoryLenientReturnPlanManifestStore inner, LenientReturnPlanManifestCache cache, int hits, int misses) : ILenientReturnPlanManifestStore
{
    public MemoryLenientReturnPlanManifestStore Inner { get; set; } = inner;
    public LenientReturnPlanManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientReturnPlanManifest? Find(string code)
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

    public void Save(LenientReturnPlanManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientReturnPlanManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient return plan manifests. Empty fields match all.</summary>
public sealed record LenientReturnPlanManifestQuery(string Label, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(LenientReturnPlanManifest item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientReturnPlanManifest> Apply(List<LenientReturnPlanManifest> items)
    {
        var matched = Collections.Filter(items, (LenientReturnPlanManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient return plan manifest store.</summary>
public static class LenientReturnPlanManifestStore
{
    public static ILenientReturnPlanManifestStore NewMemoryLenientReturnPlanManifestStore()
    {
        return new MemoryLenientReturnPlanManifestStore(
            new Dictionary<string, LenientReturnPlanManifest>(),
            new List<string>());
    }

    public static ILenientReturnPlanManifestStore NewCachedLenientReturnPlanManifestStore(MemoryLenientReturnPlanManifestStore inner, int ttl, IClock clock)
    {
        return new CachedLenientReturnPlanManifestStore(
            inner,
            LenientReturnPlanManifestCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LenientReturnPlanManifest> FindLenientReturnPlanManifests(ILenientReturnPlanManifestStore store, LenientReturnPlanManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
