namespace Acme.Workforce.Assets.LicenseSeat.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps license seat adjustments by code.</summary>
public interface ILicenseSeatAdjustmentStore
{
    LicenseSeatAdjustment? Find(string code);
    void Save(LicenseSeatAdjustment item);
    bool Remove(string code);
    List<LicenseSeatAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLicenseSeatAdjustmentStore(Dictionary<string, LicenseSeatAdjustment> items, List<string> order) : ILicenseSeatAdjustmentStore
{
    public Dictionary<string, LicenseSeatAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LicenseSeatAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LicenseSeatAdjustment item)
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

    public List<LicenseSeatAdjustment> List()
    {
        var outValue = new List<LicenseSeatAdjustment>();
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
public sealed class CachedLicenseSeatAdjustmentStore(MemoryLicenseSeatAdjustmentStore inner, LicenseSeatAdjustmentCache cache, int hits, int misses) : ILicenseSeatAdjustmentStore
{
    public MemoryLicenseSeatAdjustmentStore Inner { get; set; } = inner;
    public LicenseSeatAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LicenseSeatAdjustment? Find(string code)
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

    public void Save(LicenseSeatAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LicenseSeatAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects license seat adjustments. Empty fields match all.</summary>
public sealed record LicenseSeatAdjustmentQuery(string Name, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(LicenseSeatAdjustment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LicenseSeatAdjustment> Apply(List<LicenseSeatAdjustment> items)
    {
        var matched = Collections.Filter(items, (LicenseSeatAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of license seat adjustment store.</summary>
public static class LicenseSeatAdjustmentStore
{
    public static ILicenseSeatAdjustmentStore NewMemoryLicenseSeatAdjustmentStore()
    {
        return new MemoryLicenseSeatAdjustmentStore(
            new Dictionary<string, LicenseSeatAdjustment>(),
            new List<string>());
    }

    public static ILicenseSeatAdjustmentStore NewCachedLicenseSeatAdjustmentStore(MemoryLicenseSeatAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedLicenseSeatAdjustmentStore(inner, LicenseSeatAdjustmentCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LicenseSeatAdjustment> FindLicenseSeatAdjustments(ILicenseSeatAdjustmentStore store, LicenseSeatAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
