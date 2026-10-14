namespace Acme.Workforce.Workplace.Desk.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk profiles by code.</summary>
public interface IDeskProfileStore
{
    DeskProfile? Find(string code);
    void Save(DeskProfile item);
    bool Remove(string code);
    List<DeskProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskProfileStore(Dictionary<string, DeskProfile> items, List<string> order) : IDeskProfileStore
{
    public Dictionary<string, DeskProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskProfile item)
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

    public List<DeskProfile> List()
    {
        var outValue = new List<DeskProfile>();
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
public sealed class CachedDeskProfileStore(MemoryDeskProfileStore inner, DeskProfileCache cache, int hits, int misses) : IDeskProfileStore
{
    public MemoryDeskProfileStore Inner { get; set; } = inner;
    public DeskProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskProfile? Find(string code)
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

    public void Save(DeskProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk profiles. Empty fields match all.</summary>
public sealed record DeskProfileQuery(string Name, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(DeskProfile item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<DeskProfile> Apply(List<DeskProfile> items)
    {
        var matched = Collections.Filter(items, (DeskProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk profile store.</summary>
public static class DeskProfileStore
{
    public static IDeskProfileStore NewMemoryDeskProfileStore()
    {
        return new MemoryDeskProfileStore(new Dictionary<string, DeskProfile>(), new List<string>());
    }

    public static IDeskProfileStore NewCachedDeskProfileStore(MemoryDeskProfileStore inner, int ttl, IClock clock)
    {
        return new CachedDeskProfileStore(inner, DeskProfileCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<DeskProfile> FindDeskProfiles(IDeskProfileStore store, DeskProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
