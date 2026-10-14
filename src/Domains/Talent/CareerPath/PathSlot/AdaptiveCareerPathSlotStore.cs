namespace Acme.Workforce.Talent.CareerPath.PathSlot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive career path slots by code.</summary>
public interface IAdaptiveCareerPathSlotStore
{
    AdaptiveCareerPathSlot? Find(string code);
    void Save(AdaptiveCareerPathSlot item);
    bool Remove(string code);
    List<AdaptiveCareerPathSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveCareerPathSlotStore(Dictionary<string, AdaptiveCareerPathSlot> items, List<string> order) : IAdaptiveCareerPathSlotStore
{
    public Dictionary<string, AdaptiveCareerPathSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveCareerPathSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveCareerPathSlot item)
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

    public List<AdaptiveCareerPathSlot> List()
    {
        var outValue = new List<AdaptiveCareerPathSlot>();
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
public sealed class CachedAdaptiveCareerPathSlotStore(MemoryAdaptiveCareerPathSlotStore inner, AdaptiveCareerPathSlotCache cache, int hits, int misses) : IAdaptiveCareerPathSlotStore
{
    public MemoryAdaptiveCareerPathSlotStore Inner { get; set; } = inner;
    public AdaptiveCareerPathSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveCareerPathSlot? Find(string code)
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

    public void Save(AdaptiveCareerPathSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveCareerPathSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive career path slots. Empty fields match all.</summary>
public sealed record AdaptiveCareerPathSlotQuery(string Locale, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveCareerPathSlot item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<AdaptiveCareerPathSlot> Apply(List<AdaptiveCareerPathSlot> items)
    {
        var matched = Collections.Filter(items, (AdaptiveCareerPathSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive career path slot store.</summary>
public static class AdaptiveCareerPathSlotStore
{
    public static IAdaptiveCareerPathSlotStore NewMemoryAdaptiveCareerPathSlotStore()
    {
        return new MemoryAdaptiveCareerPathSlotStore(
            new Dictionary<string, AdaptiveCareerPathSlot>(),
            new List<string>());
    }

    public static IAdaptiveCareerPathSlotStore NewCachedAdaptiveCareerPathSlotStore(MemoryAdaptiveCareerPathSlotStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveCareerPathSlotStore(
            inner,
            AdaptiveCareerPathSlotCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveCareerPathSlot> FindAdaptiveCareerPathSlots(IAdaptiveCareerPathSlotStore store, AdaptiveCareerPathSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
