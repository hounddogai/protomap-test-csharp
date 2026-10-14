namespace Acme.Workforce.Payrollops.Garnishment.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment profiles by code.</summary>
public interface IGarnishmentProfileStore
{
    GarnishmentProfile? Find(string code);
    void Save(GarnishmentProfile item);
    bool Remove(string code);
    List<GarnishmentProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentProfileStore(Dictionary<string, GarnishmentProfile> items, List<string> order) : IGarnishmentProfileStore
{
    public Dictionary<string, GarnishmentProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentProfile item)
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

    public List<GarnishmentProfile> List()
    {
        var outValue = new List<GarnishmentProfile>();
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
public sealed class CachedGarnishmentProfileStore(MemoryGarnishmentProfileStore inner, GarnishmentProfileCache cache, int hits, int misses) : IGarnishmentProfileStore
{
    public MemoryGarnishmentProfileStore Inner { get; set; } = inner;
    public GarnishmentProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentProfile? Find(string code)
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

    public void Save(GarnishmentProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment profiles. Empty fields match all.</summary>
public sealed record GarnishmentProfileQuery(string Title, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentProfile item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<GarnishmentProfile> Apply(List<GarnishmentProfile> items)
    {
        var matched = Collections.Filter(items, (GarnishmentProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment profile store.</summary>
public static class GarnishmentProfileStore
{
    public static IGarnishmentProfileStore NewMemoryGarnishmentProfileStore()
    {
        return new MemoryGarnishmentProfileStore(new Dictionary<string, GarnishmentProfile>(), new List<string>());
    }

    public static IGarnishmentProfileStore NewCachedGarnishmentProfileStore(MemoryGarnishmentProfileStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentProfileStore(inner, GarnishmentProfileCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<GarnishmentProfile> FindGarnishmentProfiles(IGarnishmentProfileStore store, GarnishmentProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
