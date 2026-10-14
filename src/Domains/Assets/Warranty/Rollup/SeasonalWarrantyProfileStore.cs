namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal warranty profiles by code.</summary>
public interface ISeasonalWarrantyProfileStore
{
    SeasonalWarrantyProfile? Find(string code);
    void Save(SeasonalWarrantyProfile item);
    bool Remove(string code);
    List<SeasonalWarrantyProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalWarrantyProfileStore(Dictionary<string, SeasonalWarrantyProfile> items, List<string> order) : ISeasonalWarrantyProfileStore
{
    public Dictionary<string, SeasonalWarrantyProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalWarrantyProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalWarrantyProfile item)
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

    public List<SeasonalWarrantyProfile> List()
    {
        var outValue = new List<SeasonalWarrantyProfile>();
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
public sealed class CachedSeasonalWarrantyProfileStore(MemorySeasonalWarrantyProfileStore inner, SeasonalWarrantyProfileCache cache, int hits, int misses) : ISeasonalWarrantyProfileStore
{
    public MemorySeasonalWarrantyProfileStore Inner { get; set; } = inner;
    public SeasonalWarrantyProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalWarrantyProfile? Find(string code)
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

    public void Save(SeasonalWarrantyProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalWarrantyProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal warranty profiles. Empty fields match all.</summary>
public sealed record SeasonalWarrantyProfileQuery(string Team, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalWarrantyProfile item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalWarrantyProfile> Apply(List<SeasonalWarrantyProfile> items)
    {
        var matched = Collections.Filter(items, (SeasonalWarrantyProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal warranty profile store.</summary>
public static class SeasonalWarrantyProfileStore
{
    public static ISeasonalWarrantyProfileStore NewMemorySeasonalWarrantyProfileStore()
    {
        return new MemorySeasonalWarrantyProfileStore(
            new Dictionary<string, SeasonalWarrantyProfile>(),
            new List<string>());
    }

    public static ISeasonalWarrantyProfileStore NewCachedSeasonalWarrantyProfileStore(MemorySeasonalWarrantyProfileStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalWarrantyProfileStore(
            inner,
            SeasonalWarrantyProfileCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalWarrantyProfile> FindSeasonalWarrantyProfiles(ISeasonalWarrantyProfileStore store, SeasonalWarrantyProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
