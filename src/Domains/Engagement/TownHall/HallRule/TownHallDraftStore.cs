namespace Acme.Workforce.Engagement.TownHall.HallRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall drafts by code.</summary>
public interface ITownHallDraftStore
{
    TownHallDraft? Find(string code);
    void Save(TownHallDraft item);
    bool Remove(string code);
    List<TownHallDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallDraftStore(Dictionary<string, TownHallDraft> items, List<string> order) : ITownHallDraftStore
{
    public Dictionary<string, TownHallDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallDraft item)
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

    public List<TownHallDraft> List()
    {
        var outValue = new List<TownHallDraft>();
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
public sealed class CachedTownHallDraftStore(MemoryTownHallDraftStore inner, TownHallDraftCache cache, int hits, int misses) : ITownHallDraftStore
{
    public MemoryTownHallDraftStore Inner { get; set; } = inner;
    public TownHallDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallDraft? Find(string code)
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

    public void Save(TownHallDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall drafts. Empty fields match all.</summary>
public sealed record TownHallDraftQuery(string Name, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallDraft item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TownHallDraft> Apply(List<TownHallDraft> items)
    {
        var matched = Collections.Filter(items, (TownHallDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall draft store.</summary>
public static class TownHallDraftStore
{
    public static ITownHallDraftStore NewMemoryTownHallDraftStore()
    {
        return new MemoryTownHallDraftStore(new Dictionary<string, TownHallDraft>(), new List<string>());
    }

    public static ITownHallDraftStore NewCachedTownHallDraftStore(MemoryTownHallDraftStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallDraftStore(inner, TownHallDraftCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TownHallDraft> FindTownHallDrafts(ITownHallDraftStore store, TownHallDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
