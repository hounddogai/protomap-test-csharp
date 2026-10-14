namespace Acme.Workforce.Assets.Laptop.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop proposals by code.</summary>
public interface ILaptopProposalStore
{
    LaptopProposal? Find(string code);
    void Save(LaptopProposal item);
    bool Remove(string code);
    List<LaptopProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopProposalStore(Dictionary<string, LaptopProposal> items, List<string> order) : ILaptopProposalStore
{
    public Dictionary<string, LaptopProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopProposal item)
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

    public List<LaptopProposal> List()
    {
        var outValue = new List<LaptopProposal>();
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
public sealed class CachedLaptopProposalStore(MemoryLaptopProposalStore inner, LaptopProposalCache cache, int hits, int misses) : ILaptopProposalStore
{
    public MemoryLaptopProposalStore Inner { get; set; } = inner;
    public LaptopProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopProposal? Find(string code)
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

    public void Save(LaptopProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop proposals. Empty fields match all.</summary>
public sealed record LaptopProposalQuery(string Channel, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopProposal item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopProposal> Apply(List<LaptopProposal> items)
    {
        var matched = Collections.Filter(items, (LaptopProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop proposal store.</summary>
public static class LaptopProposalStore
{
    public static ILaptopProposalStore NewMemoryLaptopProposalStore()
    {
        return new MemoryLaptopProposalStore(new Dictionary<string, LaptopProposal>(), new List<string>());
    }

    public static ILaptopProposalStore NewCachedLaptopProposalStore(MemoryLaptopProposalStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopProposalStore(inner, LaptopProposalCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LaptopProposal> FindLaptopProposals(ILaptopProposalStore store, LaptopProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
