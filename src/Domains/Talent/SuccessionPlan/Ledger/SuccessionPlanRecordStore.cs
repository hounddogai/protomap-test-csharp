namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan records by code.</summary>
public interface ISuccessionPlanRecordStore
{
    SuccessionPlanRecord? Find(string code);
    void Save(SuccessionPlanRecord item);
    bool Remove(string code);
    List<SuccessionPlanRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanRecordStore(Dictionary<string, SuccessionPlanRecord> items, List<string> order) : ISuccessionPlanRecordStore
{
    public Dictionary<string, SuccessionPlanRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanRecord item)
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

    public List<SuccessionPlanRecord> List()
    {
        var outValue = new List<SuccessionPlanRecord>();
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
public sealed class CachedSuccessionPlanRecordStore(MemorySuccessionPlanRecordStore inner, SuccessionPlanRecordCache cache, int hits, int misses) : ISuccessionPlanRecordStore
{
    public MemorySuccessionPlanRecordStore Inner { get; set; } = inner;
    public SuccessionPlanRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanRecord? Find(string code)
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

    public void Save(SuccessionPlanRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan records. Empty fields match all.</summary>
public sealed record SuccessionPlanRecordQuery(string Channel, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanRecord item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<SuccessionPlanRecord> Apply(List<SuccessionPlanRecord> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan record store.</summary>
public static class SuccessionPlanRecordStore
{
    public static ISuccessionPlanRecordStore NewMemorySuccessionPlanRecordStore()
    {
        return new MemorySuccessionPlanRecordStore(
            new Dictionary<string, SuccessionPlanRecord>(),
            new List<string>());
    }

    public static ISuccessionPlanRecordStore NewCachedSuccessionPlanRecordStore(MemorySuccessionPlanRecordStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanRecordStore(inner, SuccessionPlanRecordCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<SuccessionPlanRecord> FindSuccessionPlanRecords(ISuccessionPlanRecordStore store, SuccessionPlanRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
