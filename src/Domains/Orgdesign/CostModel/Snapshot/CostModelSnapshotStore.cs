namespace Acme.Workforce.Orgdesign.CostModel.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps cost model snapshots by code.</summary>
public interface ICostModelSnapshotStore
{
    CostModelSnapshot? Find(string code);
    void Save(CostModelSnapshot item);
    bool Remove(string code);
    List<CostModelSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCostModelSnapshotStore(Dictionary<string, CostModelSnapshot> items, List<string> order) : ICostModelSnapshotStore
{
    public Dictionary<string, CostModelSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CostModelSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CostModelSnapshot item)
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

    public List<CostModelSnapshot> List()
    {
        var outValue = new List<CostModelSnapshot>();
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
public sealed class CachedCostModelSnapshotStore(MemoryCostModelSnapshotStore inner, CostModelSnapshotCache cache, int hits, int misses) : ICostModelSnapshotStore
{
    public MemoryCostModelSnapshotStore Inner { get; set; } = inner;
    public CostModelSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CostModelSnapshot? Find(string code)
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

    public void Save(CostModelSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CostModelSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects cost model snapshots. Empty fields match all.</summary>
public sealed record CostModelSnapshotQuery(string Title, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(CostModelSnapshot item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CostModelSnapshot> Apply(List<CostModelSnapshot> items)
    {
        var matched = Collections.Filter(items, (CostModelSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of cost model snapshot store.</summary>
public static class CostModelSnapshotStore
{
    public static ICostModelSnapshotStore NewMemoryCostModelSnapshotStore()
    {
        return new MemoryCostModelSnapshotStore(new Dictionary<string, CostModelSnapshot>(), new List<string>());
    }

    public static ICostModelSnapshotStore NewCachedCostModelSnapshotStore(MemoryCostModelSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedCostModelSnapshotStore(inner, CostModelSnapshotCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<CostModelSnapshot> FindCostModelSnapshots(ICostModelSnapshotStore store, CostModelSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
