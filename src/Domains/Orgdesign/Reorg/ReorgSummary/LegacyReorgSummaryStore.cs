namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy reorg summaries by code.</summary>
public interface ILegacyReorgSummaryStore
{
    LegacyReorgSummary? Find(string code);
    void Save(LegacyReorgSummary item);
    bool Remove(string code);
    List<LegacyReorgSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyReorgSummaryStore(Dictionary<string, LegacyReorgSummary> items, List<string> order) : ILegacyReorgSummaryStore
{
    public Dictionary<string, LegacyReorgSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyReorgSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyReorgSummary item)
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

    public List<LegacyReorgSummary> List()
    {
        var outValue = new List<LegacyReorgSummary>();
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
public sealed class CachedLegacyReorgSummaryStore(MemoryLegacyReorgSummaryStore inner, LegacyReorgSummaryCache cache, int hits, int misses) : ILegacyReorgSummaryStore
{
    public MemoryLegacyReorgSummaryStore Inner { get; set; } = inner;
    public LegacyReorgSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyReorgSummary? Find(string code)
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

    public void Save(LegacyReorgSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyReorgSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy reorg summaries. Empty fields match all.</summary>
public sealed record LegacyReorgSummaryQuery(string Channel, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyReorgSummary item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyReorgSummary> Apply(List<LegacyReorgSummary> items)
    {
        var matched = Collections.Filter(items, (LegacyReorgSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy reorg summary store.</summary>
public static class LegacyReorgSummaryStore
{
    public static ILegacyReorgSummaryStore NewMemoryLegacyReorgSummaryStore()
    {
        return new MemoryLegacyReorgSummaryStore(new Dictionary<string, LegacyReorgSummary>(), new List<string>());
    }

    public static ILegacyReorgSummaryStore NewCachedLegacyReorgSummaryStore(MemoryLegacyReorgSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyReorgSummaryStore(inner, LegacyReorgSummaryCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LegacyReorgSummary> FindLegacyReorgSummaries(ILegacyReorgSummaryStore store, LegacyReorgSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
