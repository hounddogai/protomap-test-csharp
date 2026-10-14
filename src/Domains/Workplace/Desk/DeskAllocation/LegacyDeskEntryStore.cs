namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy desk entries by code.</summary>
public interface ILegacyDeskEntryStore
{
    LegacyDeskEntry? Find(string code);
    void Save(LegacyDeskEntry item);
    bool Remove(string code);
    List<LegacyDeskEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyDeskEntryStore(Dictionary<string, LegacyDeskEntry> items, List<string> order) : ILegacyDeskEntryStore
{
    public Dictionary<string, LegacyDeskEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyDeskEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyDeskEntry item)
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

    public List<LegacyDeskEntry> List()
    {
        var outValue = new List<LegacyDeskEntry>();
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
public sealed class CachedLegacyDeskEntryStore(MemoryLegacyDeskEntryStore inner, LegacyDeskEntryCache cache, int hits, int misses) : ILegacyDeskEntryStore
{
    public MemoryLegacyDeskEntryStore Inner { get; set; } = inner;
    public LegacyDeskEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyDeskEntry? Find(string code)
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

    public void Save(LegacyDeskEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyDeskEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy desk entries. Empty fields match all.</summary>
public sealed record LegacyDeskEntryQuery(string Channel, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyDeskEntry item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<LegacyDeskEntry> Apply(List<LegacyDeskEntry> items)
    {
        var matched = Collections.Filter(items, (LegacyDeskEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy desk entry store.</summary>
public static class LegacyDeskEntryStore
{
    public static ILegacyDeskEntryStore NewMemoryLegacyDeskEntryStore()
    {
        return new MemoryLegacyDeskEntryStore(new Dictionary<string, LegacyDeskEntry>(), new List<string>());
    }

    public static ILegacyDeskEntryStore NewCachedLegacyDeskEntryStore(MemoryLegacyDeskEntryStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyDeskEntryStore(inner, LegacyDeskEntryCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LegacyDeskEntry> FindLegacyDeskEntries(ILegacyDeskEntryStore store, LegacyDeskEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
