namespace Acme.Workforce.Absence.Balance.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy balance adjustments by code.</summary>
public interface ILegacyBalanceAdjustmentStore
{
    LegacyBalanceAdjustment? Find(string code);
    void Save(LegacyBalanceAdjustment item);
    bool Remove(string code);
    List<LegacyBalanceAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyBalanceAdjustmentStore(Dictionary<string, LegacyBalanceAdjustment> items, List<string> order) : ILegacyBalanceAdjustmentStore
{
    public Dictionary<string, LegacyBalanceAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyBalanceAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyBalanceAdjustment item)
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

    public List<LegacyBalanceAdjustment> List()
    {
        var outValue = new List<LegacyBalanceAdjustment>();
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
public sealed class CachedLegacyBalanceAdjustmentStore(MemoryLegacyBalanceAdjustmentStore inner, LegacyBalanceAdjustmentCache cache, int hits, int misses) : ILegacyBalanceAdjustmentStore
{
    public MemoryLegacyBalanceAdjustmentStore Inner { get; set; } = inner;
    public LegacyBalanceAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyBalanceAdjustment? Find(string code)
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

    public void Save(LegacyBalanceAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyBalanceAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy balance adjustments. Empty fields match all.</summary>
public sealed record LegacyBalanceAdjustmentQuery(string Team, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyBalanceAdjustment item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyBalanceAdjustment> Apply(List<LegacyBalanceAdjustment> items)
    {
        var matched = Collections.Filter(items, (LegacyBalanceAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy balance adjustment store.</summary>
public static class LegacyBalanceAdjustmentStore
{
    public static ILegacyBalanceAdjustmentStore NewMemoryLegacyBalanceAdjustmentStore()
    {
        return new MemoryLegacyBalanceAdjustmentStore(
            new Dictionary<string, LegacyBalanceAdjustment>(),
            new List<string>());
    }

    public static ILegacyBalanceAdjustmentStore NewCachedLegacyBalanceAdjustmentStore(MemoryLegacyBalanceAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyBalanceAdjustmentStore(
            inner,
            LegacyBalanceAdjustmentCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LegacyBalanceAdjustment> FindLegacyBalanceAdjustments(ILegacyBalanceAdjustmentStore store, LegacyBalanceAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
