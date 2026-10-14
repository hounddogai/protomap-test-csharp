namespace Acme.Workforce.Assets.LicenseSeat.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy license seat windows by code.</summary>
public interface ILegacyLicenseSeatWindowStore
{
    LegacyLicenseSeatWindow? Find(string code);
    void Save(LegacyLicenseSeatWindow item);
    bool Remove(string code);
    List<LegacyLicenseSeatWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyLicenseSeatWindowStore(Dictionary<string, LegacyLicenseSeatWindow> items, List<string> order) : ILegacyLicenseSeatWindowStore
{
    public Dictionary<string, LegacyLicenseSeatWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyLicenseSeatWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyLicenseSeatWindow item)
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

    public List<LegacyLicenseSeatWindow> List()
    {
        var outValue = new List<LegacyLicenseSeatWindow>();
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
public sealed class CachedLegacyLicenseSeatWindowStore(MemoryLegacyLicenseSeatWindowStore inner, LegacyLicenseSeatWindowCache cache, int hits, int misses) : ILegacyLicenseSeatWindowStore
{
    public MemoryLegacyLicenseSeatWindowStore Inner { get; set; } = inner;
    public LegacyLicenseSeatWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyLicenseSeatWindow? Find(string code)
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

    public void Save(LegacyLicenseSeatWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyLicenseSeatWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy license seat windows. Empty fields match all.</summary>
public sealed record LegacyLicenseSeatWindowQuery(string Title, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyLicenseSeatWindow item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyLicenseSeatWindow> Apply(List<LegacyLicenseSeatWindow> items)
    {
        var matched = Collections.Filter(items, (LegacyLicenseSeatWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy license seat window store.</summary>
public static class LegacyLicenseSeatWindowStore
{
    public static ILegacyLicenseSeatWindowStore NewMemoryLegacyLicenseSeatWindowStore()
    {
        return new MemoryLegacyLicenseSeatWindowStore(
            new Dictionary<string, LegacyLicenseSeatWindow>(),
            new List<string>());
    }

    public static ILegacyLicenseSeatWindowStore NewCachedLegacyLicenseSeatWindowStore(MemoryLegacyLicenseSeatWindowStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyLicenseSeatWindowStore(
            inner,
            LegacyLicenseSeatWindowCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LegacyLicenseSeatWindow> FindLegacyLicenseSeatWindows(ILegacyLicenseSeatWindowStore store, LegacyLicenseSeatWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
