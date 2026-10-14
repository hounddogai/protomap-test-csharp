namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey mappings by code.</summary>
public interface IPulseSurveyMappingStore
{
    PulseSurveyMapping? Find(string code);
    void Save(PulseSurveyMapping item);
    bool Remove(string code);
    List<PulseSurveyMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyMappingStore(Dictionary<string, PulseSurveyMapping> items, List<string> order) : IPulseSurveyMappingStore
{
    public Dictionary<string, PulseSurveyMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyMapping item)
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

    public List<PulseSurveyMapping> List()
    {
        var outValue = new List<PulseSurveyMapping>();
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
public sealed class CachedPulseSurveyMappingStore(MemoryPulseSurveyMappingStore inner, PulseSurveyMappingCache cache, int hits, int misses) : IPulseSurveyMappingStore
{
    public MemoryPulseSurveyMappingStore Inner { get; set; } = inner;
    public PulseSurveyMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyMapping? Find(string code)
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

    public void Save(PulseSurveyMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey mappings. Empty fields match all.</summary>
public sealed record PulseSurveyMappingQuery(string Label, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyMapping item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PulseSurveyMapping> Apply(List<PulseSurveyMapping> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey mapping store.</summary>
public static class PulseSurveyMappingStore
{
    public static IPulseSurveyMappingStore NewMemoryPulseSurveyMappingStore()
    {
        return new MemoryPulseSurveyMappingStore(new Dictionary<string, PulseSurveyMapping>(), new List<string>());
    }

    public static IPulseSurveyMappingStore NewCachedPulseSurveyMappingStore(MemoryPulseSurveyMappingStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyMappingStore(inner, PulseSurveyMappingCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PulseSurveyMapping> FindPulseSurveyMappings(IPulseSurveyMappingStore store, PulseSurveyMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
