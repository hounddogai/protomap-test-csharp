namespace Acme.Workforce.Talent.Nomination.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination scores by code.</summary>
public interface INominationScoreStore
{
    NominationScore? Find(string code);
    void Save(NominationScore item);
    bool Remove(string code);
    List<NominationScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationScoreStore(Dictionary<string, NominationScore> items, List<string> order) : INominationScoreStore
{
    public Dictionary<string, NominationScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationScore item)
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

    public List<NominationScore> List()
    {
        var outValue = new List<NominationScore>();
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
public sealed class CachedNominationScoreStore(MemoryNominationScoreStore inner, NominationScoreCache cache, int hits, int misses) : INominationScoreStore
{
    public MemoryNominationScoreStore Inner { get; set; } = inner;
    public NominationScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationScore? Find(string code)
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

    public void Save(NominationScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination scores. Empty fields match all.</summary>
public sealed record NominationScoreQuery(string Unit, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(NominationScore item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<NominationScore> Apply(List<NominationScore> items)
    {
        var matched = Collections.Filter(items, (NominationScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination score store.</summary>
public static class NominationScoreStore
{
    public static INominationScoreStore NewMemoryNominationScoreStore()
    {
        return new MemoryNominationScoreStore(new Dictionary<string, NominationScore>(), new List<string>());
    }

    public static INominationScoreStore NewCachedNominationScoreStore(MemoryNominationScoreStore inner, int ttl, IClock clock)
    {
        return new CachedNominationScoreStore(inner, NominationScoreCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<NominationScore> FindNominationScores(INominationScoreStore store, NominationScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
