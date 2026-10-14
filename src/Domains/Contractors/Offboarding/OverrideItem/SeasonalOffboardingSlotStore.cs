namespace Acme.Workforce.Contractors.Offboarding.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal offboarding slots by code.</summary>
public interface ISeasonalOffboardingSlotStore
{
    SeasonalOffboardingSlot? Find(string code);
    void Save(SeasonalOffboardingSlot item);
    bool Remove(string code);
    List<SeasonalOffboardingSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalOffboardingSlotStore(Dictionary<string, SeasonalOffboardingSlot> items, List<string> order) : ISeasonalOffboardingSlotStore
{
    public Dictionary<string, SeasonalOffboardingSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalOffboardingSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalOffboardingSlot item)
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

    public List<SeasonalOffboardingSlot> List()
    {
        var outValue = new List<SeasonalOffboardingSlot>();
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
public sealed class CachedSeasonalOffboardingSlotStore(MemorySeasonalOffboardingSlotStore inner, SeasonalOffboardingSlotCache cache, int hits, int misses) : ISeasonalOffboardingSlotStore
{
    public MemorySeasonalOffboardingSlotStore Inner { get; set; } = inner;
    public SeasonalOffboardingSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalOffboardingSlot? Find(string code)
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

    public void Save(SeasonalOffboardingSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalOffboardingSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal offboarding slots. Empty fields match all.</summary>
public sealed record SeasonalOffboardingSlotQuery(string Name, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalOffboardingSlot item)
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

    public List<SeasonalOffboardingSlot> Apply(List<SeasonalOffboardingSlot> items)
    {
        var matched = Collections.Filter(items, (SeasonalOffboardingSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal offboarding slot store.</summary>
public static class SeasonalOffboardingSlotStore
{
    public static ISeasonalOffboardingSlotStore NewMemorySeasonalOffboardingSlotStore()
    {
        return new MemorySeasonalOffboardingSlotStore(
            new Dictionary<string, SeasonalOffboardingSlot>(),
            new List<string>());
    }

    public static ISeasonalOffboardingSlotStore NewCachedSeasonalOffboardingSlotStore(MemorySeasonalOffboardingSlotStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalOffboardingSlotStore(
            inner,
            SeasonalOffboardingSlotCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalOffboardingSlot> FindSeasonalOffboardingSlots(ISeasonalOffboardingSlotStore store, SeasonalOffboardingSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
