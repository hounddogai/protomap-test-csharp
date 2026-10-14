namespace Acme.Workforce.Orgdesign.Scenario.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered scenario drafts by code.</summary>
public interface ITieredScenarioDraftStore
{
    TieredScenarioDraft? Find(string code);
    void Save(TieredScenarioDraft item);
    bool Remove(string code);
    List<TieredScenarioDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredScenarioDraftStore(Dictionary<string, TieredScenarioDraft> items, List<string> order) : ITieredScenarioDraftStore
{
    public Dictionary<string, TieredScenarioDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredScenarioDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredScenarioDraft item)
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

    public List<TieredScenarioDraft> List()
    {
        var outValue = new List<TieredScenarioDraft>();
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
public sealed class CachedTieredScenarioDraftStore(MemoryTieredScenarioDraftStore inner, TieredScenarioDraftCache cache, int hits, int misses) : ITieredScenarioDraftStore
{
    public MemoryTieredScenarioDraftStore Inner { get; set; } = inner;
    public TieredScenarioDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredScenarioDraft? Find(string code)
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

    public void Save(TieredScenarioDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredScenarioDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered scenario drafts. Empty fields match all.</summary>
public sealed record TieredScenarioDraftQuery(string Label, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(TieredScenarioDraft item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<TieredScenarioDraft> Apply(List<TieredScenarioDraft> items)
    {
        var matched = Collections.Filter(items, (TieredScenarioDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered scenario draft store.</summary>
public static class TieredScenarioDraftStore
{
    public static ITieredScenarioDraftStore NewMemoryTieredScenarioDraftStore()
    {
        return new MemoryTieredScenarioDraftStore(new Dictionary<string, TieredScenarioDraft>(), new List<string>());
    }

    public static ITieredScenarioDraftStore NewCachedTieredScenarioDraftStore(MemoryTieredScenarioDraftStore inner, int ttl, IClock clock)
    {
        return new CachedTieredScenarioDraftStore(inner, TieredScenarioDraftCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TieredScenarioDraft> FindTieredScenarioDrafts(ITieredScenarioDraftStore store, TieredScenarioDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
