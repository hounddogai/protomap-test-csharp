namespace Acme.Workforce.Talent.CareerPath.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path notices by code.</summary>
public interface ICareerPathNoticeStore
{
    CareerPathNotice? Find(string code);
    void Save(CareerPathNotice item);
    bool Remove(string code);
    List<CareerPathNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathNoticeStore(Dictionary<string, CareerPathNotice> items, List<string> order) : ICareerPathNoticeStore
{
    public Dictionary<string, CareerPathNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathNotice item)
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

    public List<CareerPathNotice> List()
    {
        var outValue = new List<CareerPathNotice>();
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
public sealed class CachedCareerPathNoticeStore(MemoryCareerPathNoticeStore inner, CareerPathNoticeCache cache, int hits, int misses) : ICareerPathNoticeStore
{
    public MemoryCareerPathNoticeStore Inner { get; set; } = inner;
    public CareerPathNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathNotice? Find(string code)
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

    public void Save(CareerPathNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path notices. Empty fields match all.</summary>
public sealed record CareerPathNoticeQuery(string Label, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathNotice item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathNotice> Apply(List<CareerPathNotice> items)
    {
        var matched = Collections.Filter(items, (CareerPathNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path notice store.</summary>
public static class CareerPathNoticeStore
{
    public static ICareerPathNoticeStore NewMemoryCareerPathNoticeStore()
    {
        return new MemoryCareerPathNoticeStore(new Dictionary<string, CareerPathNotice>(), new List<string>());
    }

    public static ICareerPathNoticeStore NewCachedCareerPathNoticeStore(MemoryCareerPathNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathNoticeStore(inner, CareerPathNoticeCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<CareerPathNotice> FindCareerPathNotices(ICareerPathNoticeStore store, CareerPathNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
