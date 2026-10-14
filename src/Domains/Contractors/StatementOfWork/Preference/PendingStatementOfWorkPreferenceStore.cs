namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending statement of work preferences by code.</summary>
public interface IPendingStatementOfWorkPreferenceStore
{
    PendingStatementOfWorkPreference? Find(string code);
    void Save(PendingStatementOfWorkPreference item);
    bool Remove(string code);
    List<PendingStatementOfWorkPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingStatementOfWorkPreferenceStore(Dictionary<string, PendingStatementOfWorkPreference> items, List<string> order) : IPendingStatementOfWorkPreferenceStore
{
    public Dictionary<string, PendingStatementOfWorkPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingStatementOfWorkPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingStatementOfWorkPreference item)
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

    public List<PendingStatementOfWorkPreference> List()
    {
        var outValue = new List<PendingStatementOfWorkPreference>();
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
public sealed class CachedPendingStatementOfWorkPreferenceStore(MemoryPendingStatementOfWorkPreferenceStore inner, PendingStatementOfWorkPreferenceCache cache, int hits, int misses) : IPendingStatementOfWorkPreferenceStore
{
    public MemoryPendingStatementOfWorkPreferenceStore Inner { get; set; } = inner;
    public PendingStatementOfWorkPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingStatementOfWorkPreference? Find(string code)
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

    public void Save(PendingStatementOfWorkPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingStatementOfWorkPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending statement of work preferences. Empty fields match all.</summary>
public sealed record PendingStatementOfWorkPreferenceQuery(string Channel, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(PendingStatementOfWorkPreference item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingStatementOfWorkPreference> Apply(List<PendingStatementOfWorkPreference> items)
    {
        var matched = Collections.Filter(items, (PendingStatementOfWorkPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending statement of work preference store.</summary>
public static class PendingStatementOfWorkPreferenceStore
{
    public static IPendingStatementOfWorkPreferenceStore NewMemoryPendingStatementOfWorkPreferenceStore()
    {
        return new MemoryPendingStatementOfWorkPreferenceStore(
            new Dictionary<string, PendingStatementOfWorkPreference>(),
            new List<string>());
    }

    public static IPendingStatementOfWorkPreferenceStore NewCachedPendingStatementOfWorkPreferenceStore(MemoryPendingStatementOfWorkPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedPendingStatementOfWorkPreferenceStore(
            inner,
            PendingStatementOfWorkPreferenceCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PendingStatementOfWorkPreference> FindPendingStatementOfWorkPreferences(IPendingStatementOfWorkPreferenceStore store, PendingStatementOfWorkPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
