namespace Acme.Workforce.Talent.Nomination.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination groups by code.</summary>
public interface INominationGroupStore
{
    NominationGroup? Find(string code);
    void Save(NominationGroup item);
    bool Remove(string code);
    List<NominationGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationGroupStore(Dictionary<string, NominationGroup> items, List<string> order) : INominationGroupStore
{
    public Dictionary<string, NominationGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationGroup item)
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

    public List<NominationGroup> List()
    {
        var outValue = new List<NominationGroup>();
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
public sealed class CachedNominationGroupStore(MemoryNominationGroupStore inner, NominationGroupCache cache, int hits, int misses) : INominationGroupStore
{
    public MemoryNominationGroupStore Inner { get; set; } = inner;
    public NominationGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationGroup? Find(string code)
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

    public void Save(NominationGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination groups. Empty fields match all.</summary>
public sealed record NominationGroupQuery(string Team, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(NominationGroup item)
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

    public List<NominationGroup> Apply(List<NominationGroup> items)
    {
        var matched = Collections.Filter(items, (NominationGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination group store.</summary>
public static class NominationGroupStore
{
    public static INominationGroupStore NewMemoryNominationGroupStore()
    {
        return new MemoryNominationGroupStore(new Dictionary<string, NominationGroup>(), new List<string>());
    }

    public static INominationGroupStore NewCachedNominationGroupStore(MemoryNominationGroupStore inner, int ttl, IClock clock)
    {
        return new CachedNominationGroupStore(inner, NominationGroupCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<NominationGroup> FindNominationGroups(INominationGroupStore store, NominationGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
