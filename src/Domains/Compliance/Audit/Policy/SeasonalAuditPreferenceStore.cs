namespace Acme.Workforce.Compliance.Audit.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal audit preferences by code.</summary>
public interface ISeasonalAuditPreferenceStore
{
    SeasonalAuditPreference? Find(string code);
    void Save(SeasonalAuditPreference item);
    bool Remove(string code);
    List<SeasonalAuditPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalAuditPreferenceStore(Dictionary<string, SeasonalAuditPreference> items, List<string> order) : ISeasonalAuditPreferenceStore
{
    public Dictionary<string, SeasonalAuditPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalAuditPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalAuditPreference item)
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

    public List<SeasonalAuditPreference> List()
    {
        var outValue = new List<SeasonalAuditPreference>();
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
public sealed class CachedSeasonalAuditPreferenceStore(MemorySeasonalAuditPreferenceStore inner, SeasonalAuditPreferenceCache cache, int hits, int misses) : ISeasonalAuditPreferenceStore
{
    public MemorySeasonalAuditPreferenceStore Inner { get; set; } = inner;
    public SeasonalAuditPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalAuditPreference? Find(string code)
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

    public void Save(SeasonalAuditPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalAuditPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal audit preferences. Empty fields match all.</summary>
public sealed record SeasonalAuditPreferenceQuery(string Segment, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalAuditPreference item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalAuditPreference> Apply(List<SeasonalAuditPreference> items)
    {
        var matched = Collections.Filter(items, (SeasonalAuditPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal audit preference store.</summary>
public static class SeasonalAuditPreferenceStore
{
    public static ISeasonalAuditPreferenceStore NewMemorySeasonalAuditPreferenceStore()
    {
        return new MemorySeasonalAuditPreferenceStore(
            new Dictionary<string, SeasonalAuditPreference>(),
            new List<string>());
    }

    public static ISeasonalAuditPreferenceStore NewCachedSeasonalAuditPreferenceStore(MemorySeasonalAuditPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalAuditPreferenceStore(
            inner,
            SeasonalAuditPreferenceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalAuditPreference> FindSeasonalAuditPreferences(ISeasonalAuditPreferenceStore store, SeasonalAuditPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
