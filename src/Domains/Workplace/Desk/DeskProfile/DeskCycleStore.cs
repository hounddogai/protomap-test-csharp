namespace Acme.Workforce.Workplace.Desk.DeskProfile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk cycles by code.</summary>
public interface IDeskCycleStore
{
    DeskCycle? Find(string code);
    void Save(DeskCycle item);
    bool Remove(string code);
    List<DeskCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskCycleStore(Dictionary<string, DeskCycle> items, List<string> order) : IDeskCycleStore
{
    public Dictionary<string, DeskCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskCycle item)
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

    public List<DeskCycle> List()
    {
        var outValue = new List<DeskCycle>();
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
public sealed class CachedDeskCycleStore(MemoryDeskCycleStore inner, DeskCycleCache cache, int hits, int misses) : IDeskCycleStore
{
    public MemoryDeskCycleStore Inner { get; set; } = inner;
    public DeskCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskCycle? Find(string code)
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

    public void Save(DeskCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk cycles. Empty fields match all.</summary>
public sealed record DeskCycleQuery(string Title, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(DeskCycle item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskCycle> Apply(List<DeskCycle> items)
    {
        var matched = Collections.Filter(items, (DeskCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk cycle store.</summary>
public static class DeskCycleStore
{
    public static IDeskCycleStore NewMemoryDeskCycleStore()
    {
        return new MemoryDeskCycleStore(new Dictionary<string, DeskCycle>(), new List<string>());
    }

    public static IDeskCycleStore NewCachedDeskCycleStore(MemoryDeskCycleStore inner, int ttl, IClock clock)
    {
        return new CachedDeskCycleStore(inner, DeskCycleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DeskCycle> FindDeskCycles(IDeskCycleStore store, DeskCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
