namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile proposals by code.</summary>
public interface ISkillProfileProposalStore
{
    SkillProfileProposal? Find(string code);
    void Save(SkillProfileProposal item);
    bool Remove(string code);
    List<SkillProfileProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfileProposalStore(Dictionary<string, SkillProfileProposal> items, List<string> order) : ISkillProfileProposalStore
{
    public Dictionary<string, SkillProfileProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfileProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfileProposal item)
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

    public List<SkillProfileProposal> List()
    {
        var outValue = new List<SkillProfileProposal>();
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
public sealed class CachedSkillProfileProposalStore(MemorySkillProfileProposalStore inner, SkillProfileProposalCache cache, int hits, int misses) : ISkillProfileProposalStore
{
    public MemorySkillProfileProposalStore Inner { get; set; } = inner;
    public SkillProfileProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfileProposal? Find(string code)
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

    public void Save(SkillProfileProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfileProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile proposals. Empty fields match all.</summary>
public sealed record SkillProfileProposalQuery(string Category, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfileProposal item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<SkillProfileProposal> Apply(List<SkillProfileProposal> items)
    {
        var matched = Collections.Filter(items, (SkillProfileProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile proposal store.</summary>
public static class SkillProfileProposalStore
{
    public static ISkillProfileProposalStore NewMemorySkillProfileProposalStore()
    {
        return new MemorySkillProfileProposalStore(
            new Dictionary<string, SkillProfileProposal>(),
            new List<string>());
    }

    public static ISkillProfileProposalStore NewCachedSkillProfileProposalStore(MemorySkillProfileProposalStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfileProposalStore(inner, SkillProfileProposalCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SkillProfileProposal> FindSkillProfileProposals(ISkillProfileProposalStore store, SkillProfileProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
