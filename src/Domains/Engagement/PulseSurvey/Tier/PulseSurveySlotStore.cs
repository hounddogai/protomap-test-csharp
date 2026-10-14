namespace Acme.Workforce.Engagement.PulseSurvey.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey slots by code.</summary>
public interface IPulseSurveySlotStore
{
    PulseSurveySlot? Find(string code);
    void Save(PulseSurveySlot item);
    bool Remove(string code);
    List<PulseSurveySlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveySlotStore(Dictionary<string, PulseSurveySlot> items, List<string> order) : IPulseSurveySlotStore
{
    public Dictionary<string, PulseSurveySlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveySlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveySlot item)
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

    public List<PulseSurveySlot> List()
    {
        var outValue = new List<PulseSurveySlot>();
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
public sealed class CachedPulseSurveySlotStore(MemoryPulseSurveySlotStore inner, PulseSurveySlotCache cache, int hits, int misses) : IPulseSurveySlotStore
{
    public MemoryPulseSurveySlotStore Inner { get; set; } = inner;
    public PulseSurveySlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveySlot? Find(string code)
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

    public void Save(PulseSurveySlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveySlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey slots. Empty fields match all.</summary>
public sealed record PulseSurveySlotQuery(string Name, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveySlot item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<PulseSurveySlot> Apply(List<PulseSurveySlot> items)
    {
        var matched = Collections.Filter(items, (PulseSurveySlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey slot store.</summary>
public static class PulseSurveySlotStore
{
    public static IPulseSurveySlotStore NewMemoryPulseSurveySlotStore()
    {
        return new MemoryPulseSurveySlotStore(new Dictionary<string, PulseSurveySlot>(), new List<string>());
    }

    public static IPulseSurveySlotStore NewCachedPulseSurveySlotStore(MemoryPulseSurveySlotStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveySlotStore(inner, PulseSurveySlotCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PulseSurveySlot> FindPulseSurveySlots(IPulseSurveySlotStore store, PulseSurveySlotQuery query)
    {
        return query.Apply(store.List());
    }
}
