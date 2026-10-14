namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal span profiles by code.</summary>
public interface ISeasonalSpanProfileStore
{
    SeasonalSpanProfile? Find(string code);
    void Save(SeasonalSpanProfile item);
    bool Remove(string code);
    List<SeasonalSpanProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalSpanProfileStore(Dictionary<string, SeasonalSpanProfile> items, List<string> order) : ISeasonalSpanProfileStore
{
    public Dictionary<string, SeasonalSpanProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalSpanProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalSpanProfile item)
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

    public List<SeasonalSpanProfile> List()
    {
        var outValue = new List<SeasonalSpanProfile>();
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
public sealed class CachedSeasonalSpanProfileStore(MemorySeasonalSpanProfileStore inner, SeasonalSpanProfileCache cache, int hits, int misses) : ISeasonalSpanProfileStore
{
    public MemorySeasonalSpanProfileStore Inner { get; set; } = inner;
    public SeasonalSpanProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalSpanProfile? Find(string code)
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

    public void Save(SeasonalSpanProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalSpanProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal span profiles. Empty fields match all.</summary>
public sealed record SeasonalSpanProfileQuery(string Name, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalSpanProfile item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<SeasonalSpanProfile> Apply(List<SeasonalSpanProfile> items)
    {
        var matched = Collections.Filter(items, (SeasonalSpanProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal span profile store.</summary>
public static class SeasonalSpanProfileStore
{
    public static ISeasonalSpanProfileStore NewMemorySeasonalSpanProfileStore()
    {
        return new MemorySeasonalSpanProfileStore(new Dictionary<string, SeasonalSpanProfile>(), new List<string>());
    }

    public static ISeasonalSpanProfileStore NewCachedSeasonalSpanProfileStore(MemorySeasonalSpanProfileStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalSpanProfileStore(inner, SeasonalSpanProfileCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<SeasonalSpanProfile> FindSeasonalSpanProfiles(ISeasonalSpanProfileStore store, SeasonalSpanProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
