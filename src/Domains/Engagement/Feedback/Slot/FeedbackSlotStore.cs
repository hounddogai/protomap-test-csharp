namespace Acme.Workforce.Engagement.Feedback.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback slots by code.</summary>
public interface IFeedbackSlotStore
{
    FeedbackSlot? Find(string code);
    void Save(FeedbackSlot item);
    bool Remove(string code);
    List<FeedbackSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackSlotStore(Dictionary<string, FeedbackSlot> items, List<string> order) : IFeedbackSlotStore
{
    public Dictionary<string, FeedbackSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackSlot item)
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

    public List<FeedbackSlot> List()
    {
        var outValue = new List<FeedbackSlot>();
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
public sealed class CachedFeedbackSlotStore(MemoryFeedbackSlotStore inner, FeedbackSlotCache cache, int hits, int misses) : IFeedbackSlotStore
{
    public MemoryFeedbackSlotStore Inner { get; set; } = inner;
    public FeedbackSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackSlot? Find(string code)
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

    public void Save(FeedbackSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback slots. Empty fields match all.</summary>
public sealed record FeedbackSlotQuery(string Channel, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackSlot item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FeedbackSlot> Apply(List<FeedbackSlot> items)
    {
        var matched = Collections.Filter(items, (FeedbackSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback slot store.</summary>
public static class FeedbackSlotStore
{
    public static IFeedbackSlotStore NewMemoryFeedbackSlotStore()
    {
        return new MemoryFeedbackSlotStore(new Dictionary<string, FeedbackSlot>(), new List<string>());
    }

    public static IFeedbackSlotStore NewCachedFeedbackSlotStore(MemoryFeedbackSlotStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackSlotStore(inner, FeedbackSlotCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<FeedbackSlot> FindFeedbackSlots(IFeedbackSlotStore store, FeedbackSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
