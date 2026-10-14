namespace Acme.Workforce.Workplace.Desk.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk windows by code.</summary>
public interface IDeskWindowStore
{
    DeskWindow? Find(string code);
    void Save(DeskWindow item);
    bool Remove(string code);
    List<DeskWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskWindowStore(Dictionary<string, DeskWindow> items, List<string> order) : IDeskWindowStore
{
    public Dictionary<string, DeskWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskWindow item)
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

    public List<DeskWindow> List()
    {
        var outValue = new List<DeskWindow>();
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
public sealed class CachedDeskWindowStore(MemoryDeskWindowStore inner, DeskWindowCache cache, int hits, int misses) : IDeskWindowStore
{
    public MemoryDeskWindowStore Inner { get; set; } = inner;
    public DeskWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskWindow? Find(string code)
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

    public void Save(DeskWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk windows. Empty fields match all.</summary>
public sealed record DeskWindowQuery(string Team, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(DeskWindow item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskWindow> Apply(List<DeskWindow> items)
    {
        var matched = Collections.Filter(items, (DeskWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk window store.</summary>
public static class DeskWindowStore
{
    public static IDeskWindowStore NewMemoryDeskWindowStore()
    {
        return new MemoryDeskWindowStore(new Dictionary<string, DeskWindow>(), new List<string>());
    }

    public static IDeskWindowStore NewCachedDeskWindowStore(MemoryDeskWindowStore inner, int ttl, IClock clock)
    {
        return new CachedDeskWindowStore(inner, DeskWindowCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<DeskWindow> FindDeskWindows(IDeskWindowStore store, DeskWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
