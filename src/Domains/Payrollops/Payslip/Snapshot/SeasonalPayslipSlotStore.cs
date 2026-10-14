namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal payslip slots by code.</summary>
public interface ISeasonalPayslipSlotStore
{
    SeasonalPayslipSlot? Find(string code);
    void Save(SeasonalPayslipSlot item);
    bool Remove(string code);
    List<SeasonalPayslipSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalPayslipSlotStore(Dictionary<string, SeasonalPayslipSlot> items, List<string> order) : ISeasonalPayslipSlotStore
{
    public Dictionary<string, SeasonalPayslipSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalPayslipSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalPayslipSlot item)
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

    public List<SeasonalPayslipSlot> List()
    {
        var outValue = new List<SeasonalPayslipSlot>();
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
public sealed class CachedSeasonalPayslipSlotStore(MemorySeasonalPayslipSlotStore inner, SeasonalPayslipSlotCache cache, int hits, int misses) : ISeasonalPayslipSlotStore
{
    public MemorySeasonalPayslipSlotStore Inner { get; set; } = inner;
    public SeasonalPayslipSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalPayslipSlot? Find(string code)
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

    public void Save(SeasonalPayslipSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalPayslipSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal payslip slots. Empty fields match all.</summary>
public sealed record SeasonalPayslipSlotQuery(string Team, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalPayslipSlot item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalPayslipSlot> Apply(List<SeasonalPayslipSlot> items)
    {
        var matched = Collections.Filter(items, (SeasonalPayslipSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal payslip slot store.</summary>
public static class SeasonalPayslipSlotStore
{
    public static ISeasonalPayslipSlotStore NewMemorySeasonalPayslipSlotStore()
    {
        return new MemorySeasonalPayslipSlotStore(new Dictionary<string, SeasonalPayslipSlot>(), new List<string>());
    }

    public static ISeasonalPayslipSlotStore NewCachedSeasonalPayslipSlotStore(MemorySeasonalPayslipSlotStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalPayslipSlotStore(inner, SeasonalPayslipSlotCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SeasonalPayslipSlot> FindSeasonalPayslipSlots(ISeasonalPayslipSlotStore store, SeasonalPayslipSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
