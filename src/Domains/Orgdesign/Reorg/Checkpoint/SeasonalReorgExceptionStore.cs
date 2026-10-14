namespace Acme.Workforce.Orgdesign.Reorg.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal reorg exceptions by code.</summary>
public interface ISeasonalReorgExceptionStore
{
    SeasonalReorgException? Find(string code);
    void Save(SeasonalReorgException item);
    bool Remove(string code);
    List<SeasonalReorgException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalReorgExceptionStore(Dictionary<string, SeasonalReorgException> items, List<string> order) : ISeasonalReorgExceptionStore
{
    public Dictionary<string, SeasonalReorgException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalReorgException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalReorgException item)
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

    public List<SeasonalReorgException> List()
    {
        var outValue = new List<SeasonalReorgException>();
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
public sealed class CachedSeasonalReorgExceptionStore(MemorySeasonalReorgExceptionStore inner, SeasonalReorgExceptionCache cache, int hits, int misses) : ISeasonalReorgExceptionStore
{
    public MemorySeasonalReorgExceptionStore Inner { get; set; } = inner;
    public SeasonalReorgExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalReorgException? Find(string code)
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

    public void Save(SeasonalReorgException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalReorgException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal reorg exceptions. Empty fields match all.</summary>
public sealed record SeasonalReorgExceptionQuery(string Label, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalReorgException item)
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

    public List<SeasonalReorgException> Apply(List<SeasonalReorgException> items)
    {
        var matched = Collections.Filter(items, (SeasonalReorgException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal reorg exception store.</summary>
public static class SeasonalReorgExceptionStore
{
    public static ISeasonalReorgExceptionStore NewMemorySeasonalReorgExceptionStore()
    {
        return new MemorySeasonalReorgExceptionStore(
            new Dictionary<string, SeasonalReorgException>(),
            new List<string>());
    }

    public static ISeasonalReorgExceptionStore NewCachedSeasonalReorgExceptionStore(MemorySeasonalReorgExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalReorgExceptionStore(
            inner,
            SeasonalReorgExceptionCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalReorgException> FindSeasonalReorgExceptions(ISeasonalReorgExceptionStore store, SeasonalReorgExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
