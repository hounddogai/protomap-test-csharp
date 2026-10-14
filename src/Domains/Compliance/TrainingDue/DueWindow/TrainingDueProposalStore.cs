namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due proposals by code.</summary>
public interface ITrainingDueProposalStore
{
    TrainingDueProposal? Find(string code);
    void Save(TrainingDueProposal item);
    bool Remove(string code);
    List<TrainingDueProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueProposalStore(Dictionary<string, TrainingDueProposal> items, List<string> order) : ITrainingDueProposalStore
{
    public Dictionary<string, TrainingDueProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueProposal item)
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

    public List<TrainingDueProposal> List()
    {
        var outValue = new List<TrainingDueProposal>();
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
public sealed class CachedTrainingDueProposalStore(MemoryTrainingDueProposalStore inner, TrainingDueProposalCache cache, int hits, int misses) : ITrainingDueProposalStore
{
    public MemoryTrainingDueProposalStore Inner { get; set; } = inner;
    public TrainingDueProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueProposal? Find(string code)
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

    public void Save(TrainingDueProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due proposals. Empty fields match all.</summary>
public sealed record TrainingDueProposalQuery(string Name, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueProposal item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TrainingDueProposal> Apply(List<TrainingDueProposal> items)
    {
        var matched = Collections.Filter(items, (TrainingDueProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due proposal store.</summary>
public static class TrainingDueProposalStore
{
    public static ITrainingDueProposalStore NewMemoryTrainingDueProposalStore()
    {
        return new MemoryTrainingDueProposalStore(new Dictionary<string, TrainingDueProposal>(), new List<string>());
    }

    public static ITrainingDueProposalStore NewCachedTrainingDueProposalStore(MemoryTrainingDueProposalStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueProposalStore(inner, TrainingDueProposalCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TrainingDueProposal> FindTrainingDueProposals(ITrainingDueProposalStore store, TrainingDueProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
