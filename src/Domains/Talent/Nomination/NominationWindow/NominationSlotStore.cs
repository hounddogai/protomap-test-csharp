namespace Acme.Workforce.Talent.Nomination.NominationWindow;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination slots by code.</summary>
public interface INominationSlotStore
{
    NominationSlot? Find(string code);
    void Save(NominationSlot item);
    bool Remove(string code);
    List<NominationSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationSlotStore(Dictionary<string, NominationSlot> items, List<string> order) : INominationSlotStore
{
    public Dictionary<string, NominationSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationSlot item)
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

    public List<NominationSlot> List()
    {
        var outValue = new List<NominationSlot>();
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
public sealed class CachedNominationSlotStore(MemoryNominationSlotStore inner, NominationSlotCache cache, int hits, int misses) : INominationSlotStore
{
    public MemoryNominationSlotStore Inner { get; set; } = inner;
    public NominationSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationSlot? Find(string code)
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

    public void Save(NominationSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination slots. Empty fields match all.</summary>
public sealed record NominationSlotQuery(string Team, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(NominationSlot item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<NominationSlot> Apply(List<NominationSlot> items)
    {
        var matched = Collections.Filter(items, (NominationSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination slot store.</summary>
public static class NominationSlotStore
{
    public static INominationSlotStore NewMemoryNominationSlotStore()
    {
        return new MemoryNominationSlotStore(new Dictionary<string, NominationSlot>(), new List<string>());
    }

    public static INominationSlotStore NewCachedNominationSlotStore(MemoryNominationSlotStore inner, int ttl, IClock clock)
    {
        return new CachedNominationSlotStore(inner, NominationSlotCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<NominationSlot> FindNominationSlots(INominationSlotStore store, NominationSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
