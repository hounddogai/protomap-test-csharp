namespace Acme.Workforce.Assets.AssetReturn.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return agreements by code.</summary>
public interface IAssetReturnAgreementStore
{
    AssetReturnAgreement? Find(string code);
    void Save(AssetReturnAgreement item);
    bool Remove(string code);
    List<AssetReturnAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnAgreementStore(Dictionary<string, AssetReturnAgreement> items, List<string> order) : IAssetReturnAgreementStore
{
    public Dictionary<string, AssetReturnAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnAgreement item)
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

    public List<AssetReturnAgreement> List()
    {
        var outValue = new List<AssetReturnAgreement>();
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
public sealed class CachedAssetReturnAgreementStore(MemoryAssetReturnAgreementStore inner, AssetReturnAgreementCache cache, int hits, int misses) : IAssetReturnAgreementStore
{
    public MemoryAssetReturnAgreementStore Inner { get; set; } = inner;
    public AssetReturnAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnAgreement? Find(string code)
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

    public void Save(AssetReturnAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return agreements. Empty fields match all.</summary>
public sealed record AssetReturnAgreementQuery(string Team, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnAgreement item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<AssetReturnAgreement> Apply(List<AssetReturnAgreement> items)
    {
        var matched = Collections.Filter(items, (AssetReturnAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return agreement store.</summary>
public static class AssetReturnAgreementStore
{
    public static IAssetReturnAgreementStore NewMemoryAssetReturnAgreementStore()
    {
        return new MemoryAssetReturnAgreementStore(
            new Dictionary<string, AssetReturnAgreement>(),
            new List<string>());
    }

    public static IAssetReturnAgreementStore NewCachedAssetReturnAgreementStore(MemoryAssetReturnAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnAgreementStore(inner, AssetReturnAgreementCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AssetReturnAgreement> FindAssetReturnAgreements(IAssetReturnAgreementStore store, AssetReturnAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
