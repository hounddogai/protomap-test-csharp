namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal tax form tiers by code.</summary>
public interface ISeasonalTaxFormTierStore
{
    SeasonalTaxFormTier? Find(string code);
    void Save(SeasonalTaxFormTier item);
    bool Remove(string code);
    List<SeasonalTaxFormTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalTaxFormTierStore(Dictionary<string, SeasonalTaxFormTier> items, List<string> order) : ISeasonalTaxFormTierStore
{
    public Dictionary<string, SeasonalTaxFormTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalTaxFormTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalTaxFormTier item)
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

    public List<SeasonalTaxFormTier> List()
    {
        var outValue = new List<SeasonalTaxFormTier>();
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
public sealed class CachedSeasonalTaxFormTierStore(MemorySeasonalTaxFormTierStore inner, SeasonalTaxFormTierCache cache, int hits, int misses) : ISeasonalTaxFormTierStore
{
    public MemorySeasonalTaxFormTierStore Inner { get; set; } = inner;
    public SeasonalTaxFormTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalTaxFormTier? Find(string code)
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

    public void Save(SeasonalTaxFormTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalTaxFormTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal tax form tiers. Empty fields match all.</summary>
public sealed record SeasonalTaxFormTierQuery(string Team, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalTaxFormTier item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalTaxFormTier> Apply(List<SeasonalTaxFormTier> items)
    {
        var matched = Collections.Filter(items, (SeasonalTaxFormTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal tax form tier store.</summary>
public static class SeasonalTaxFormTierStore
{
    public static ISeasonalTaxFormTierStore NewMemorySeasonalTaxFormTierStore()
    {
        return new MemorySeasonalTaxFormTierStore(new Dictionary<string, SeasonalTaxFormTier>(), new List<string>());
    }

    public static ISeasonalTaxFormTierStore NewCachedSeasonalTaxFormTierStore(MemorySeasonalTaxFormTierStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalTaxFormTierStore(inner, SeasonalTaxFormTierCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SeasonalTaxFormTier> FindSeasonalTaxFormTiers(ISeasonalTaxFormTierStore store, SeasonalTaxFormTierQuery query)
    {
        return query.Apply(store.List());
    }
}
