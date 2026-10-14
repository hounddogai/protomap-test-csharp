namespace Acme.Workforce.Workplace.Visitor.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor profiles by code.</summary>
public interface IVisitorProfileStore
{
    VisitorProfile? Find(string code);
    void Save(VisitorProfile item);
    bool Remove(string code);
    List<VisitorProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorProfileStore(Dictionary<string, VisitorProfile> items, List<string> order) : IVisitorProfileStore
{
    public Dictionary<string, VisitorProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorProfile item)
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

    public List<VisitorProfile> List()
    {
        var outValue = new List<VisitorProfile>();
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
public sealed class CachedVisitorProfileStore(MemoryVisitorProfileStore inner, VisitorProfileCache cache, int hits, int misses) : IVisitorProfileStore
{
    public MemoryVisitorProfileStore Inner { get; set; } = inner;
    public VisitorProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorProfile? Find(string code)
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

    public void Save(VisitorProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor profiles. Empty fields match all.</summary>
public sealed record VisitorProfileQuery(string Label, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorProfile item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorProfile> Apply(List<VisitorProfile> items)
    {
        var matched = Collections.Filter(items, (VisitorProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor profile store.</summary>
public static class VisitorProfileStore
{
    public static IVisitorProfileStore NewMemoryVisitorProfileStore()
    {
        return new MemoryVisitorProfileStore(new Dictionary<string, VisitorProfile>(), new List<string>());
    }

    public static IVisitorProfileStore NewCachedVisitorProfileStore(MemoryVisitorProfileStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorProfileStore(inner, VisitorProfileCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<VisitorProfile> FindVisitorProfiles(IVisitorProfileStore store, VisitorProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
