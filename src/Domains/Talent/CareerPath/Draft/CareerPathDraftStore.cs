namespace Acme.Workforce.Talent.CareerPath.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path drafts by code.</summary>
public interface ICareerPathDraftStore
{
    CareerPathDraft? Find(string code);
    void Save(CareerPathDraft item);
    bool Remove(string code);
    List<CareerPathDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathDraftStore(Dictionary<string, CareerPathDraft> items, List<string> order) : ICareerPathDraftStore
{
    public Dictionary<string, CareerPathDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathDraft item)
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

    public List<CareerPathDraft> List()
    {
        var outValue = new List<CareerPathDraft>();
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
public sealed class CachedCareerPathDraftStore(MemoryCareerPathDraftStore inner, CareerPathDraftCache cache, int hits, int misses) : ICareerPathDraftStore
{
    public MemoryCareerPathDraftStore Inner { get; set; } = inner;
    public CareerPathDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathDraft? Find(string code)
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

    public void Save(CareerPathDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path drafts. Empty fields match all.</summary>
public sealed record CareerPathDraftQuery(string Segment, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathDraft item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathDraft> Apply(List<CareerPathDraft> items)
    {
        var matched = Collections.Filter(items, (CareerPathDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path draft store.</summary>
public static class CareerPathDraftStore
{
    public static ICareerPathDraftStore NewMemoryCareerPathDraftStore()
    {
        return new MemoryCareerPathDraftStore(new Dictionary<string, CareerPathDraft>(), new List<string>());
    }

    public static ICareerPathDraftStore NewCachedCareerPathDraftStore(MemoryCareerPathDraftStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathDraftStore(inner, CareerPathDraftCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<CareerPathDraft> FindCareerPathDrafts(ICareerPathDraftStore store, CareerPathDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
