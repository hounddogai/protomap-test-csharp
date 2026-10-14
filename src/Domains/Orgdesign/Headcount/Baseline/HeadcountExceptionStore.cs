namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount exceptions by code.</summary>
public interface IHeadcountExceptionStore
{
    HeadcountException? Find(string code);
    void Save(HeadcountException item);
    bool Remove(string code);
    List<HeadcountException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountExceptionStore(Dictionary<string, HeadcountException> items, List<string> order) : IHeadcountExceptionStore
{
    public Dictionary<string, HeadcountException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountException item)
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

    public List<HeadcountException> List()
    {
        var outValue = new List<HeadcountException>();
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
public sealed class CachedHeadcountExceptionStore(MemoryHeadcountExceptionStore inner, HeadcountExceptionCache cache, int hits, int misses) : IHeadcountExceptionStore
{
    public MemoryHeadcountExceptionStore Inner { get; set; } = inner;
    public HeadcountExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountException? Find(string code)
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

    public void Save(HeadcountException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount exceptions. Empty fields match all.</summary>
public sealed record HeadcountExceptionQuery(string Team, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountException item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountException> Apply(List<HeadcountException> items)
    {
        var matched = Collections.Filter(items, (HeadcountException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount exception store.</summary>
public static class HeadcountExceptionStore
{
    public static IHeadcountExceptionStore NewMemoryHeadcountExceptionStore()
    {
        return new MemoryHeadcountExceptionStore(new Dictionary<string, HeadcountException>(), new List<string>());
    }

    public static IHeadcountExceptionStore NewCachedHeadcountExceptionStore(MemoryHeadcountExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountExceptionStore(inner, HeadcountExceptionCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<HeadcountException> FindHeadcountExceptions(IHeadcountExceptionStore store, HeadcountExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
