namespace Acme.Workforce.Talent.Nomination.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal nomination cycles by code.</summary>
public interface ISeasonalNominationCycleStore
{
    SeasonalNominationCycle? Find(string code);
    void Save(SeasonalNominationCycle item);
    bool Remove(string code);
    List<SeasonalNominationCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalNominationCycleStore(Dictionary<string, SeasonalNominationCycle> items, List<string> order) : ISeasonalNominationCycleStore
{
    public Dictionary<string, SeasonalNominationCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalNominationCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalNominationCycle item)
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

    public List<SeasonalNominationCycle> List()
    {
        var outValue = new List<SeasonalNominationCycle>();
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
public sealed class CachedSeasonalNominationCycleStore(MemorySeasonalNominationCycleStore inner, SeasonalNominationCycleCache cache, int hits, int misses) : ISeasonalNominationCycleStore
{
    public MemorySeasonalNominationCycleStore Inner { get; set; } = inner;
    public SeasonalNominationCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalNominationCycle? Find(string code)
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

    public void Save(SeasonalNominationCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalNominationCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal nomination cycles. Empty fields match all.</summary>
public sealed record SeasonalNominationCycleQuery(string Source, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalNominationCycle item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalNominationCycle> Apply(List<SeasonalNominationCycle> items)
    {
        var matched = Collections.Filter(items, (SeasonalNominationCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal nomination cycle store.</summary>
public static class SeasonalNominationCycleStore
{
    public static ISeasonalNominationCycleStore NewMemorySeasonalNominationCycleStore()
    {
        return new MemorySeasonalNominationCycleStore(
            new Dictionary<string, SeasonalNominationCycle>(),
            new List<string>());
    }

    public static ISeasonalNominationCycleStore NewCachedSeasonalNominationCycleStore(MemorySeasonalNominationCycleStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalNominationCycleStore(
            inner,
            SeasonalNominationCycleCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalNominationCycle> FindSeasonalNominationCycles(ISeasonalNominationCycleStore store, SeasonalNominationCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
