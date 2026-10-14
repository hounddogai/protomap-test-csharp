namespace Acme.Workforce.Talent.CareerPath.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path proposals by code.</summary>
public interface ICareerPathProposalStore
{
    CareerPathProposal? Find(string code);
    void Save(CareerPathProposal item);
    bool Remove(string code);
    List<CareerPathProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathProposalStore(Dictionary<string, CareerPathProposal> items, List<string> order) : ICareerPathProposalStore
{
    public Dictionary<string, CareerPathProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathProposal item)
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

    public List<CareerPathProposal> List()
    {
        var outValue = new List<CareerPathProposal>();
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
public sealed class CachedCareerPathProposalStore(MemoryCareerPathProposalStore inner, CareerPathProposalCache cache, int hits, int misses) : ICareerPathProposalStore
{
    public MemoryCareerPathProposalStore Inner { get; set; } = inner;
    public CareerPathProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathProposal? Find(string code)
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

    public void Save(CareerPathProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path proposals. Empty fields match all.</summary>
public sealed record CareerPathProposalQuery(string Name, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathProposal item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathProposal> Apply(List<CareerPathProposal> items)
    {
        var matched = Collections.Filter(items, (CareerPathProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path proposal store.</summary>
public static class CareerPathProposalStore
{
    public static ICareerPathProposalStore NewMemoryCareerPathProposalStore()
    {
        return new MemoryCareerPathProposalStore(new Dictionary<string, CareerPathProposal>(), new List<string>());
    }

    public static ICareerPathProposalStore NewCachedCareerPathProposalStore(MemoryCareerPathProposalStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathProposalStore(inner, CareerPathProposalCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CareerPathProposal> FindCareerPathProposals(ICareerPathProposalStore store, CareerPathProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
