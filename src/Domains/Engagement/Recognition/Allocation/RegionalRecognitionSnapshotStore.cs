namespace Acme.Workforce.Engagement.Recognition.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional recognition snapshots by code.</summary>
public interface IRegionalRecognitionSnapshotStore
{
    RegionalRecognitionSnapshot? Find(string code);
    void Save(RegionalRecognitionSnapshot item);
    bool Remove(string code);
    List<RegionalRecognitionSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalRecognitionSnapshotStore(Dictionary<string, RegionalRecognitionSnapshot> items, List<string> order) : IRegionalRecognitionSnapshotStore
{
    public Dictionary<string, RegionalRecognitionSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalRecognitionSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalRecognitionSnapshot item)
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

    public List<RegionalRecognitionSnapshot> List()
    {
        var outValue = new List<RegionalRecognitionSnapshot>();
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
public sealed class CachedRegionalRecognitionSnapshotStore(MemoryRegionalRecognitionSnapshotStore inner, RegionalRecognitionSnapshotCache cache, int hits, int misses) : IRegionalRecognitionSnapshotStore
{
    public MemoryRegionalRecognitionSnapshotStore Inner { get; set; } = inner;
    public RegionalRecognitionSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalRecognitionSnapshot? Find(string code)
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

    public void Save(RegionalRecognitionSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalRecognitionSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional recognition snapshots. Empty fields match all.</summary>
public sealed record RegionalRecognitionSnapshotQuery(string Category, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalRecognitionSnapshot item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalRecognitionSnapshot> Apply(List<RegionalRecognitionSnapshot> items)
    {
        var matched = Collections.Filter(items, (RegionalRecognitionSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional recognition snapshot store.</summary>
public static class RegionalRecognitionSnapshotStore
{
    public static IRegionalRecognitionSnapshotStore NewMemoryRegionalRecognitionSnapshotStore()
    {
        return new MemoryRegionalRecognitionSnapshotStore(
            new Dictionary<string, RegionalRecognitionSnapshot>(),
            new List<string>());
    }

    public static IRegionalRecognitionSnapshotStore NewCachedRegionalRecognitionSnapshotStore(MemoryRegionalRecognitionSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalRecognitionSnapshotStore(
            inner,
            RegionalRecognitionSnapshotCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<RegionalRecognitionSnapshot> FindRegionalRecognitionSnapshots(IRegionalRecognitionSnapshotStore store, RegionalRecognitionSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
