namespace Acme.Workforce.Orgdesign.Scenario.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario proposals by code.</summary>
public interface IScenarioProposalStore
{
    ScenarioProposal? Find(string code);
    void Save(ScenarioProposal item);
    bool Remove(string code);
    List<ScenarioProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioProposalStore(Dictionary<string, ScenarioProposal> items, List<string> order) : IScenarioProposalStore
{
    public Dictionary<string, ScenarioProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioProposal item)
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

    public List<ScenarioProposal> List()
    {
        var outValue = new List<ScenarioProposal>();
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
public sealed class CachedScenarioProposalStore(MemoryScenarioProposalStore inner, ScenarioProposalCache cache, int hits, int misses) : IScenarioProposalStore
{
    public MemoryScenarioProposalStore Inner { get; set; } = inner;
    public ScenarioProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioProposal? Find(string code)
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

    public void Save(ScenarioProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario proposals. Empty fields match all.</summary>
public sealed record ScenarioProposalQuery(string Title, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioProposal item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScenarioProposal> Apply(List<ScenarioProposal> items)
    {
        var matched = Collections.Filter(items, (ScenarioProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario proposal store.</summary>
public static class ScenarioProposalStore
{
    public static IScenarioProposalStore NewMemoryScenarioProposalStore()
    {
        return new MemoryScenarioProposalStore(new Dictionary<string, ScenarioProposal>(), new List<string>());
    }

    public static IScenarioProposalStore NewCachedScenarioProposalStore(MemoryScenarioProposalStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioProposalStore(inner, ScenarioProposalCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ScenarioProposal> FindScenarioProposals(IScenarioProposalStore store, ScenarioProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
