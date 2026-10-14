namespace Acme.Workforce.Orgdesign.Span.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span entries by code.</summary>
public interface ISpanEntryStore
{
    SpanEntry? Find(string code);
    void Save(SpanEntry item);
    bool Remove(string code);
    List<SpanEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanEntryStore(Dictionary<string, SpanEntry> items, List<string> order) : ISpanEntryStore
{
    public Dictionary<string, SpanEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanEntry item)
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

    public List<SpanEntry> List()
    {
        var outValue = new List<SpanEntry>();
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
public sealed class CachedSpanEntryStore(MemorySpanEntryStore inner, SpanEntryCache cache, int hits, int misses) : ISpanEntryStore
{
    public MemorySpanEntryStore Inner { get; set; } = inner;
    public SpanEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanEntry? Find(string code)
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

    public void Save(SpanEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span entries. Empty fields match all.</summary>
public sealed record SpanEntryQuery(string Team, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(SpanEntry item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanEntry> Apply(List<SpanEntry> items)
    {
        var matched = Collections.Filter(items, (SpanEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span entry store.</summary>
public static class SpanEntryStore
{
    public static ISpanEntryStore NewMemorySpanEntryStore()
    {
        return new MemorySpanEntryStore(new Dictionary<string, SpanEntry>(), new List<string>());
    }

    public static ISpanEntryStore NewCachedSpanEntryStore(MemorySpanEntryStore inner, int ttl, IClock clock)
    {
        return new CachedSpanEntryStore(inner, SpanEntryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SpanEntry> FindSpanEntries(ISpanEntryStore store, SpanEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
