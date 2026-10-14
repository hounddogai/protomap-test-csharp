namespace Acme.Workforce.Orgdesign.Span.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span slots by code.</summary>
public interface ISpanSlotStore
{
    SpanSlot? Find(string code);
    void Save(SpanSlot item);
    bool Remove(string code);
    List<SpanSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanSlotStore(Dictionary<string, SpanSlot> items, List<string> order) : ISpanSlotStore
{
    public Dictionary<string, SpanSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanSlot item)
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

    public List<SpanSlot> List()
    {
        var outValue = new List<SpanSlot>();
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
public sealed class CachedSpanSlotStore(MemorySpanSlotStore inner, SpanSlotCache cache, int hits, int misses) : ISpanSlotStore
{
    public MemorySpanSlotStore Inner { get; set; } = inner;
    public SpanSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanSlot? Find(string code)
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

    public void Save(SpanSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span slots. Empty fields match all.</summary>
public sealed record SpanSlotQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(SpanSlot item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<SpanSlot> Apply(List<SpanSlot> items)
    {
        var matched = Collections.Filter(items, (SpanSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span slot store.</summary>
public static class SpanSlotStore
{
    public static ISpanSlotStore NewMemorySpanSlotStore()
    {
        return new MemorySpanSlotStore(new Dictionary<string, SpanSlot>(), new List<string>());
    }

    public static ISpanSlotStore NewCachedSpanSlotStore(MemorySpanSlotStore inner, int ttl, IClock clock)
    {
        return new CachedSpanSlotStore(inner, SpanSlotCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<SpanSlot> FindSpanSlots(ISpanSlotStore store, SpanSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
