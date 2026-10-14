namespace Acme.Workforce.Talent.Nomination.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination bundles by code.</summary>
public interface INominationBundleStore
{
    NominationBundle? Find(string code);
    void Save(NominationBundle item);
    bool Remove(string code);
    List<NominationBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationBundleStore(Dictionary<string, NominationBundle> items, List<string> order) : INominationBundleStore
{
    public Dictionary<string, NominationBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationBundle item)
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

    public List<NominationBundle> List()
    {
        var outValue = new List<NominationBundle>();
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
public sealed class CachedNominationBundleStore(MemoryNominationBundleStore inner, NominationBundleCache cache, int hits, int misses) : INominationBundleStore
{
    public MemoryNominationBundleStore Inner { get; set; } = inner;
    public NominationBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationBundle? Find(string code)
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

    public void Save(NominationBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination bundles. Empty fields match all.</summary>
public sealed record NominationBundleQuery(string Department, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(NominationBundle item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<NominationBundle> Apply(List<NominationBundle> items)
    {
        var matched = Collections.Filter(items, (NominationBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination bundle store.</summary>
public static class NominationBundleStore
{
    public static INominationBundleStore NewMemoryNominationBundleStore()
    {
        return new MemoryNominationBundleStore(new Dictionary<string, NominationBundle>(), new List<string>());
    }

    public static INominationBundleStore NewCachedNominationBundleStore(MemoryNominationBundleStore inner, int ttl, IClock clock)
    {
        return new CachedNominationBundleStore(inner, NominationBundleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<NominationBundle> FindNominationBundles(INominationBundleStore store, NominationBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
