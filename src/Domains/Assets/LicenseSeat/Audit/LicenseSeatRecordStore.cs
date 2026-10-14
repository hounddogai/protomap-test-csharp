namespace Acme.Workforce.Assets.LicenseSeat.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps license seat records by code.</summary>
public interface ILicenseSeatRecordStore
{
    LicenseSeatRecord? Find(string code);
    void Save(LicenseSeatRecord item);
    bool Remove(string code);
    List<LicenseSeatRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLicenseSeatRecordStore(Dictionary<string, LicenseSeatRecord> items, List<string> order) : ILicenseSeatRecordStore
{
    public Dictionary<string, LicenseSeatRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LicenseSeatRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LicenseSeatRecord item)
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

    public List<LicenseSeatRecord> List()
    {
        var outValue = new List<LicenseSeatRecord>();
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
public sealed class CachedLicenseSeatRecordStore(MemoryLicenseSeatRecordStore inner, LicenseSeatRecordCache cache, int hits, int misses) : ILicenseSeatRecordStore
{
    public MemoryLicenseSeatRecordStore Inner { get; set; } = inner;
    public LicenseSeatRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LicenseSeatRecord? Find(string code)
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

    public void Save(LicenseSeatRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LicenseSeatRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects license seat records. Empty fields match all.</summary>
public sealed record LicenseSeatRecordQuery(string Name, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(LicenseSeatRecord item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LicenseSeatRecord> Apply(List<LicenseSeatRecord> items)
    {
        var matched = Collections.Filter(items, (LicenseSeatRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of license seat record store.</summary>
public static class LicenseSeatRecordStore
{
    public static ILicenseSeatRecordStore NewMemoryLicenseSeatRecordStore()
    {
        return new MemoryLicenseSeatRecordStore(new Dictionary<string, LicenseSeatRecord>(), new List<string>());
    }

    public static ILicenseSeatRecordStore NewCachedLicenseSeatRecordStore(MemoryLicenseSeatRecordStore inner, int ttl, IClock clock)
    {
        return new CachedLicenseSeatRecordStore(inner, LicenseSeatRecordCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LicenseSeatRecord> FindLicenseSeatRecords(ILicenseSeatRecordStore store, LicenseSeatRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
