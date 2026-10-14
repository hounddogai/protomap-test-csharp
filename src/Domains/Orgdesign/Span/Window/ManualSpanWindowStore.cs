namespace Acme.Workforce.Orgdesign.Span.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual span windows by code.</summary>
public interface IManualSpanWindowStore
{
    ManualSpanWindow? Find(string code);
    void Save(ManualSpanWindow item);
    bool Remove(string code);
    List<ManualSpanWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualSpanWindowStore(Dictionary<string, ManualSpanWindow> items, List<string> order) : IManualSpanWindowStore
{
    public Dictionary<string, ManualSpanWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualSpanWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualSpanWindow item)
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

    public List<ManualSpanWindow> List()
    {
        var outValue = new List<ManualSpanWindow>();
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
public sealed class CachedManualSpanWindowStore(MemoryManualSpanWindowStore inner, ManualSpanWindowCache cache, int hits, int misses) : IManualSpanWindowStore
{
    public MemoryManualSpanWindowStore Inner { get; set; } = inner;
    public ManualSpanWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualSpanWindow? Find(string code)
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

    public void Save(ManualSpanWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualSpanWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual span windows. Empty fields match all.</summary>
public sealed record ManualSpanWindowQuery(string Title, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(ManualSpanWindow item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<ManualSpanWindow> Apply(List<ManualSpanWindow> items)
    {
        var matched = Collections.Filter(items, (ManualSpanWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual span window store.</summary>
public static class ManualSpanWindowStore
{
    public static IManualSpanWindowStore NewMemoryManualSpanWindowStore()
    {
        return new MemoryManualSpanWindowStore(new Dictionary<string, ManualSpanWindow>(), new List<string>());
    }

    public static IManualSpanWindowStore NewCachedManualSpanWindowStore(MemoryManualSpanWindowStore inner, int ttl, IClock clock)
    {
        return new CachedManualSpanWindowStore(inner, ManualSpanWindowCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ManualSpanWindow> FindManualSpanWindows(IManualSpanWindowStore store, ManualSpanWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
