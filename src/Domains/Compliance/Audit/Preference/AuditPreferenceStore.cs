namespace Acme.Workforce.Compliance.Audit.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit preferences by code.</summary>
public interface IAuditPreferenceStore
{
    AuditPreference? Find(string code);
    void Save(AuditPreference item);
    bool Remove(string code);
    List<AuditPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditPreferenceStore(Dictionary<string, AuditPreference> items, List<string> order) : IAuditPreferenceStore
{
    public Dictionary<string, AuditPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditPreference item)
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

    public List<AuditPreference> List()
    {
        var outValue = new List<AuditPreference>();
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
public sealed class CachedAuditPreferenceStore(MemoryAuditPreferenceStore inner, AuditPreferenceCache cache, int hits, int misses) : IAuditPreferenceStore
{
    public MemoryAuditPreferenceStore Inner { get; set; } = inner;
    public AuditPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditPreference? Find(string code)
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

    public void Save(AuditPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit preferences. Empty fields match all.</summary>
public sealed record AuditPreferenceQuery(string Label, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(AuditPreference item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<AuditPreference> Apply(List<AuditPreference> items)
    {
        var matched = Collections.Filter(items, (AuditPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit preference store.</summary>
public static class AuditPreferenceStore
{
    public static IAuditPreferenceStore NewMemoryAuditPreferenceStore()
    {
        return new MemoryAuditPreferenceStore(new Dictionary<string, AuditPreference>(), new List<string>());
    }

    public static IAuditPreferenceStore NewCachedAuditPreferenceStore(MemoryAuditPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedAuditPreferenceStore(inner, AuditPreferenceCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AuditPreference> FindAuditPreferences(IAuditPreferenceStore store, AuditPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
