namespace Acme.Workforce.Absence.Accrual.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual baselines by code.</summary>
public interface IAccrualBaselineStore
{
    AccrualBaseline? Find(string code);
    void Save(AccrualBaseline item);
    bool Remove(string code);
    List<AccrualBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualBaselineStore(Dictionary<string, AccrualBaseline> items, List<string> order) : IAccrualBaselineStore
{
    public Dictionary<string, AccrualBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualBaseline item)
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

    public List<AccrualBaseline> List()
    {
        var outValue = new List<AccrualBaseline>();
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
public sealed class CachedAccrualBaselineStore(MemoryAccrualBaselineStore inner, AccrualBaselineCache cache, int hits, int misses) : IAccrualBaselineStore
{
    public MemoryAccrualBaselineStore Inner { get; set; } = inner;
    public AccrualBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualBaseline? Find(string code)
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

    public void Save(AccrualBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual baselines. Empty fields match all.</summary>
public sealed record AccrualBaselineQuery(string Title, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualBaseline item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<AccrualBaseline> Apply(List<AccrualBaseline> items)
    {
        var matched = Collections.Filter(items, (AccrualBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual baseline store.</summary>
public static class AccrualBaselineStore
{
    public static IAccrualBaselineStore NewMemoryAccrualBaselineStore()
    {
        return new MemoryAccrualBaselineStore(new Dictionary<string, AccrualBaseline>(), new List<string>());
    }

    public static IAccrualBaselineStore NewCachedAccrualBaselineStore(MemoryAccrualBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualBaselineStore(inner, AccrualBaselineCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AccrualBaseline> FindAccrualBaselines(IAccrualBaselineStore store, AccrualBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
