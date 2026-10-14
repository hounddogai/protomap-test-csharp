namespace Acme.Workforce.Talent.SkillProfile.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled skill profile proposals by code.</summary>
public interface IScheduledSkillProfileProposalStore
{
    ScheduledSkillProfileProposal? Find(string code);
    void Save(ScheduledSkillProfileProposal item);
    bool Remove(string code);
    List<ScheduledSkillProfileProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledSkillProfileProposalStore(Dictionary<string, ScheduledSkillProfileProposal> items, List<string> order) : IScheduledSkillProfileProposalStore
{
    public Dictionary<string, ScheduledSkillProfileProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledSkillProfileProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledSkillProfileProposal item)
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

    public List<ScheduledSkillProfileProposal> List()
    {
        var outValue = new List<ScheduledSkillProfileProposal>();
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
public sealed class CachedScheduledSkillProfileProposalStore(MemoryScheduledSkillProfileProposalStore inner, ScheduledSkillProfileProposalCache cache, int hits, int misses) : IScheduledSkillProfileProposalStore
{
    public MemoryScheduledSkillProfileProposalStore Inner { get; set; } = inner;
    public ScheduledSkillProfileProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledSkillProfileProposal? Find(string code)
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

    public void Save(ScheduledSkillProfileProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledSkillProfileProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled skill profile proposals. Empty fields match all.</summary>
public sealed record ScheduledSkillProfileProposalQuery(string Label, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledSkillProfileProposal item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<ScheduledSkillProfileProposal> Apply(List<ScheduledSkillProfileProposal> items)
    {
        var matched = Collections.Filter(items, (ScheduledSkillProfileProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled skill profile proposal store.</summary>
public static class ScheduledSkillProfileProposalStore
{
    public static IScheduledSkillProfileProposalStore NewMemoryScheduledSkillProfileProposalStore()
    {
        return new MemoryScheduledSkillProfileProposalStore(
            new Dictionary<string, ScheduledSkillProfileProposal>(),
            new List<string>());
    }

    public static IScheduledSkillProfileProposalStore NewCachedScheduledSkillProfileProposalStore(MemoryScheduledSkillProfileProposalStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledSkillProfileProposalStore(
            inner,
            ScheduledSkillProfileProposalCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledSkillProfileProposal> FindScheduledSkillProfileProposals(IScheduledSkillProfileProposalStore store, ScheduledSkillProfileProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
