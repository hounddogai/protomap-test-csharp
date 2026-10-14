namespace Acme.Workforce.Compensation.MeritCycle.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal merit cycle entries by code.</summary>
public interface ISeasonalMeritCycleEntryStore
{
    SeasonalMeritCycleEntry? Find(string code);
    void Save(SeasonalMeritCycleEntry item);
    bool Remove(string code);
    List<SeasonalMeritCycleEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalMeritCycleEntryStore(Dictionary<string, SeasonalMeritCycleEntry> items, List<string> order) : ISeasonalMeritCycleEntryStore
{
    public Dictionary<string, SeasonalMeritCycleEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalMeritCycleEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalMeritCycleEntry item)
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

    public List<SeasonalMeritCycleEntry> List()
    {
        var outValue = new List<SeasonalMeritCycleEntry>();
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
public sealed class CachedSeasonalMeritCycleEntryStore(MemorySeasonalMeritCycleEntryStore inner, SeasonalMeritCycleEntryCache cache, int hits, int misses) : ISeasonalMeritCycleEntryStore
{
    public MemorySeasonalMeritCycleEntryStore Inner { get; set; } = inner;
    public SeasonalMeritCycleEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalMeritCycleEntry? Find(string code)
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

    public void Save(SeasonalMeritCycleEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalMeritCycleEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal merit cycle entries. Empty fields match all.</summary>
public sealed record SeasonalMeritCycleEntryQuery(string Label, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalMeritCycleEntry item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalMeritCycleEntry> Apply(List<SeasonalMeritCycleEntry> items)
    {
        var matched = Collections.Filter(items, (SeasonalMeritCycleEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal merit cycle entry store.</summary>
public static class SeasonalMeritCycleEntryStore
{
    public static ISeasonalMeritCycleEntryStore NewMemorySeasonalMeritCycleEntryStore()
    {
        return new MemorySeasonalMeritCycleEntryStore(
            new Dictionary<string, SeasonalMeritCycleEntry>(),
            new List<string>());
    }

    public static ISeasonalMeritCycleEntryStore NewCachedSeasonalMeritCycleEntryStore(MemorySeasonalMeritCycleEntryStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalMeritCycleEntryStore(
            inner,
            SeasonalMeritCycleEntryCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalMeritCycleEntry> FindSeasonalMeritCycleEntries(ISeasonalMeritCycleEntryStore store, SeasonalMeritCycleEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
