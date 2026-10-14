namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk baselines by code.</summary>
public interface IDeskBaselineStore
{
    DeskBaseline? Find(string code);
    void Save(DeskBaseline item);
    bool Remove(string code);
    List<DeskBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskBaselineStore(Dictionary<string, DeskBaseline> items, List<string> order) : IDeskBaselineStore
{
    public Dictionary<string, DeskBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskBaseline item)
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

    public List<DeskBaseline> List()
    {
        var outValue = new List<DeskBaseline>();
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
public sealed class CachedDeskBaselineStore(MemoryDeskBaselineStore inner, DeskBaselineCache cache, int hits, int misses) : IDeskBaselineStore
{
    public MemoryDeskBaselineStore Inner { get; set; } = inner;
    public DeskBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskBaseline? Find(string code)
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

    public void Save(DeskBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk baselines. Empty fields match all.</summary>
public sealed record DeskBaselineQuery(string Unit, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(DeskBaseline item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskBaseline> Apply(List<DeskBaseline> items)
    {
        var matched = Collections.Filter(items, (DeskBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk baseline store.</summary>
public static class DeskBaselineStore
{
    public static IDeskBaselineStore NewMemoryDeskBaselineStore()
    {
        return new MemoryDeskBaselineStore(new Dictionary<string, DeskBaseline>(), new List<string>());
    }

    public static IDeskBaselineStore NewCachedDeskBaselineStore(MemoryDeskBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedDeskBaselineStore(inner, DeskBaselineCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<DeskBaseline> FindDeskBaselines(IDeskBaselineStore store, DeskBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
