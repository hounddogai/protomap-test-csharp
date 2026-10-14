namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey milestones by code.</summary>
public interface IPulseSurveyMilestoneStore
{
    PulseSurveyMilestone? Find(string code);
    void Save(PulseSurveyMilestone item);
    bool Remove(string code);
    List<PulseSurveyMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyMilestoneStore(Dictionary<string, PulseSurveyMilestone> items, List<string> order) : IPulseSurveyMilestoneStore
{
    public Dictionary<string, PulseSurveyMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyMilestone item)
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

    public List<PulseSurveyMilestone> List()
    {
        var outValue = new List<PulseSurveyMilestone>();
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
public sealed class CachedPulseSurveyMilestoneStore(MemoryPulseSurveyMilestoneStore inner, PulseSurveyMilestoneCache cache, int hits, int misses) : IPulseSurveyMilestoneStore
{
    public MemoryPulseSurveyMilestoneStore Inner { get; set; } = inner;
    public PulseSurveyMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyMilestone? Find(string code)
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

    public void Save(PulseSurveyMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey milestones. Empty fields match all.</summary>
public sealed record PulseSurveyMilestoneQuery(string Label, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyMilestone item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PulseSurveyMilestone> Apply(List<PulseSurveyMilestone> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey milestone store.</summary>
public static class PulseSurveyMilestoneStore
{
    public static IPulseSurveyMilestoneStore NewMemoryPulseSurveyMilestoneStore()
    {
        return new MemoryPulseSurveyMilestoneStore(
            new Dictionary<string, PulseSurveyMilestone>(),
            new List<string>());
    }

    public static IPulseSurveyMilestoneStore NewCachedPulseSurveyMilestoneStore(MemoryPulseSurveyMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyMilestoneStore(inner, PulseSurveyMilestoneCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PulseSurveyMilestone> FindPulseSurveyMilestones(IPulseSurveyMilestoneStore store, PulseSurveyMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
