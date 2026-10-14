namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk exceptions by code.</summary>
public interface IDeskExceptionStore
{
    DeskException? Find(string code);
    void Save(DeskException item);
    bool Remove(string code);
    List<DeskException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskExceptionStore(Dictionary<string, DeskException> items, List<string> order) : IDeskExceptionStore
{
    public Dictionary<string, DeskException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskException item)
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

    public List<DeskException> List()
    {
        var outValue = new List<DeskException>();
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
public sealed class CachedDeskExceptionStore(MemoryDeskExceptionStore inner, DeskExceptionCache cache, int hits, int misses) : IDeskExceptionStore
{
    public MemoryDeskExceptionStore Inner { get; set; } = inner;
    public DeskExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskException? Find(string code)
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

    public void Save(DeskException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk exceptions. Empty fields match all.</summary>
public sealed record DeskExceptionQuery(string Title, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(DeskException item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskException> Apply(List<DeskException> items)
    {
        var matched = Collections.Filter(items, (DeskException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk exception store.</summary>
public static class DeskExceptionStore
{
    public static IDeskExceptionStore NewMemoryDeskExceptionStore()
    {
        return new MemoryDeskExceptionStore(new Dictionary<string, DeskException>(), new List<string>());
    }

    public static IDeskExceptionStore NewCachedDeskExceptionStore(MemoryDeskExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedDeskExceptionStore(inner, DeskExceptionCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<DeskException> FindDeskExceptions(IDeskExceptionStore store, DeskExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
