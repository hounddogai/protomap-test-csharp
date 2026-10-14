namespace Acme.Workforce.Talent.SkillProfile.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal skill profile cycles by code.</summary>
public interface ISeasonalSkillProfileCycleStore
{
    SeasonalSkillProfileCycle? Find(string code);
    void Save(SeasonalSkillProfileCycle item);
    bool Remove(string code);
    List<SeasonalSkillProfileCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalSkillProfileCycleStore(Dictionary<string, SeasonalSkillProfileCycle> items, List<string> order) : ISeasonalSkillProfileCycleStore
{
    public Dictionary<string, SeasonalSkillProfileCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalSkillProfileCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalSkillProfileCycle item)
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

    public List<SeasonalSkillProfileCycle> List()
    {
        var outValue = new List<SeasonalSkillProfileCycle>();
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
public sealed class CachedSeasonalSkillProfileCycleStore(MemorySeasonalSkillProfileCycleStore inner, SeasonalSkillProfileCycleCache cache, int hits, int misses) : ISeasonalSkillProfileCycleStore
{
    public MemorySeasonalSkillProfileCycleStore Inner { get; set; } = inner;
    public SeasonalSkillProfileCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalSkillProfileCycle? Find(string code)
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

    public void Save(SeasonalSkillProfileCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalSkillProfileCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal skill profile cycles. Empty fields match all.</summary>
public sealed record SeasonalSkillProfileCycleQuery(string Label, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalSkillProfileCycle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalSkillProfileCycle> Apply(List<SeasonalSkillProfileCycle> items)
    {
        var matched = Collections.Filter(items, (SeasonalSkillProfileCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal skill profile cycle store.</summary>
public static class SeasonalSkillProfileCycleStore
{
    public static ISeasonalSkillProfileCycleStore NewMemorySeasonalSkillProfileCycleStore()
    {
        return new MemorySeasonalSkillProfileCycleStore(
            new Dictionary<string, SeasonalSkillProfileCycle>(),
            new List<string>());
    }

    public static ISeasonalSkillProfileCycleStore NewCachedSeasonalSkillProfileCycleStore(MemorySeasonalSkillProfileCycleStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalSkillProfileCycleStore(
            inner,
            SeasonalSkillProfileCycleCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalSkillProfileCycle> FindSeasonalSkillProfileCycles(ISeasonalSkillProfileCycleStore store, SeasonalSkillProfileCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
