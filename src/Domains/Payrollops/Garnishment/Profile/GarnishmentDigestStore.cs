namespace Acme.Workforce.Payrollops.Garnishment.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment digests by code.</summary>
public interface IGarnishmentDigestStore
{
    GarnishmentDigest? Find(string code);
    void Save(GarnishmentDigest item);
    bool Remove(string code);
    List<GarnishmentDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentDigestStore(Dictionary<string, GarnishmentDigest> items, List<string> order) : IGarnishmentDigestStore
{
    public Dictionary<string, GarnishmentDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentDigest item)
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

    public List<GarnishmentDigest> List()
    {
        var outValue = new List<GarnishmentDigest>();
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
public sealed class CachedGarnishmentDigestStore(MemoryGarnishmentDigestStore inner, GarnishmentDigestCache cache, int hits, int misses) : IGarnishmentDigestStore
{
    public MemoryGarnishmentDigestStore Inner { get; set; } = inner;
    public GarnishmentDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentDigest? Find(string code)
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

    public void Save(GarnishmentDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment digests. Empty fields match all.</summary>
public sealed record GarnishmentDigestQuery(string Label, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentDigest item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<GarnishmentDigest> Apply(List<GarnishmentDigest> items)
    {
        var matched = Collections.Filter(items, (GarnishmentDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment digest store.</summary>
public static class GarnishmentDigestStore
{
    public static IGarnishmentDigestStore NewMemoryGarnishmentDigestStore()
    {
        return new MemoryGarnishmentDigestStore(new Dictionary<string, GarnishmentDigest>(), new List<string>());
    }

    public static IGarnishmentDigestStore NewCachedGarnishmentDigestStore(MemoryGarnishmentDigestStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentDigestStore(inner, GarnishmentDigestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<GarnishmentDigest> FindGarnishmentDigests(IGarnishmentDigestStore store, GarnishmentDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
