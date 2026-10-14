namespace Acme.Workforce.Compliance.Attestation.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global attestation baselines by code.</summary>
public interface IGlobalAttestationBaselineStore
{
    GlobalAttestationBaseline? Find(string code);
    void Save(GlobalAttestationBaseline item);
    bool Remove(string code);
    List<GlobalAttestationBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalAttestationBaselineStore(Dictionary<string, GlobalAttestationBaseline> items, List<string> order) : IGlobalAttestationBaselineStore
{
    public Dictionary<string, GlobalAttestationBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalAttestationBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalAttestationBaseline item)
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

    public List<GlobalAttestationBaseline> List()
    {
        var outValue = new List<GlobalAttestationBaseline>();
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
public sealed class CachedGlobalAttestationBaselineStore(MemoryGlobalAttestationBaselineStore inner, GlobalAttestationBaselineCache cache, int hits, int misses) : IGlobalAttestationBaselineStore
{
    public MemoryGlobalAttestationBaselineStore Inner { get; set; } = inner;
    public GlobalAttestationBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalAttestationBaseline? Find(string code)
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

    public void Save(GlobalAttestationBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalAttestationBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global attestation baselines. Empty fields match all.</summary>
public sealed record GlobalAttestationBaselineQuery(string Label, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalAttestationBaseline item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<GlobalAttestationBaseline> Apply(List<GlobalAttestationBaseline> items)
    {
        var matched = Collections.Filter(items, (GlobalAttestationBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global attestation baseline store.</summary>
public static class GlobalAttestationBaselineStore
{
    public static IGlobalAttestationBaselineStore NewMemoryGlobalAttestationBaselineStore()
    {
        return new MemoryGlobalAttestationBaselineStore(
            new Dictionary<string, GlobalAttestationBaseline>(),
            new List<string>());
    }

    public static IGlobalAttestationBaselineStore NewCachedGlobalAttestationBaselineStore(MemoryGlobalAttestationBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalAttestationBaselineStore(
            inner,
            GlobalAttestationBaselineCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<GlobalAttestationBaseline> FindGlobalAttestationBaselines(IGlobalAttestationBaselineStore store, GlobalAttestationBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
