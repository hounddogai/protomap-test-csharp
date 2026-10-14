namespace Acme.Workforce.Engagement.ActionPlan.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient action plan records by code.</summary>
public interface ILenientActionPlanRecordStore
{
    LenientActionPlanRecord? Find(string code);
    void Save(LenientActionPlanRecord item);
    bool Remove(string code);
    List<LenientActionPlanRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientActionPlanRecordStore(Dictionary<string, LenientActionPlanRecord> items, List<string> order) : ILenientActionPlanRecordStore
{
    public Dictionary<string, LenientActionPlanRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientActionPlanRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientActionPlanRecord item)
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

    public List<LenientActionPlanRecord> List()
    {
        var outValue = new List<LenientActionPlanRecord>();
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
public sealed class CachedLenientActionPlanRecordStore(MemoryLenientActionPlanRecordStore inner, LenientActionPlanRecordCache cache, int hits, int misses) : ILenientActionPlanRecordStore
{
    public MemoryLenientActionPlanRecordStore Inner { get; set; } = inner;
    public LenientActionPlanRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientActionPlanRecord? Find(string code)
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

    public void Save(LenientActionPlanRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientActionPlanRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient action plan records. Empty fields match all.</summary>
public sealed record LenientActionPlanRecordQuery(string Name, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(LenientActionPlanRecord item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientActionPlanRecord> Apply(List<LenientActionPlanRecord> items)
    {
        var matched = Collections.Filter(items, (LenientActionPlanRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient action plan record store.</summary>
public static class LenientActionPlanRecordStore
{
    public static ILenientActionPlanRecordStore NewMemoryLenientActionPlanRecordStore()
    {
        return new MemoryLenientActionPlanRecordStore(
            new Dictionary<string, LenientActionPlanRecord>(),
            new List<string>());
    }

    public static ILenientActionPlanRecordStore NewCachedLenientActionPlanRecordStore(MemoryLenientActionPlanRecordStore inner, int ttl, IClock clock)
    {
        return new CachedLenientActionPlanRecordStore(
            inner,
            LenientActionPlanRecordCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LenientActionPlanRecord> FindLenientActionPlanRecords(ILenientActionPlanRecordStore store, LenientActionPlanRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
