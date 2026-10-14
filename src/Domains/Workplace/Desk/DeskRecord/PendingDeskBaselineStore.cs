namespace Acme.Workforce.Workplace.Desk.DeskRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending desk baselines by code.</summary>
public interface IPendingDeskBaselineStore
{
    PendingDeskBaseline? Find(string code);
    void Save(PendingDeskBaseline item);
    bool Remove(string code);
    List<PendingDeskBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingDeskBaselineStore(Dictionary<string, PendingDeskBaseline> items, List<string> order) : IPendingDeskBaselineStore
{
    public Dictionary<string, PendingDeskBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingDeskBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingDeskBaseline item)
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

    public List<PendingDeskBaseline> List()
    {
        var outValue = new List<PendingDeskBaseline>();
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
public sealed class CachedPendingDeskBaselineStore(MemoryPendingDeskBaselineStore inner, PendingDeskBaselineCache cache, int hits, int misses) : IPendingDeskBaselineStore
{
    public MemoryPendingDeskBaselineStore Inner { get; set; } = inner;
    public PendingDeskBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingDeskBaseline? Find(string code)
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

    public void Save(PendingDeskBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingDeskBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending desk baselines. Empty fields match all.</summary>
public sealed record PendingDeskBaselineQuery(string Name, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(PendingDeskBaseline item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<PendingDeskBaseline> Apply(List<PendingDeskBaseline> items)
    {
        var matched = Collections.Filter(items, (PendingDeskBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending desk baseline store.</summary>
public static class PendingDeskBaselineStore
{
    public static IPendingDeskBaselineStore NewMemoryPendingDeskBaselineStore()
    {
        return new MemoryPendingDeskBaselineStore(new Dictionary<string, PendingDeskBaseline>(), new List<string>());
    }

    public static IPendingDeskBaselineStore NewCachedPendingDeskBaselineStore(MemoryPendingDeskBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedPendingDeskBaselineStore(inner, PendingDeskBaselineCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PendingDeskBaseline> FindPendingDeskBaselines(IPendingDeskBaselineStore store, PendingDeskBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
