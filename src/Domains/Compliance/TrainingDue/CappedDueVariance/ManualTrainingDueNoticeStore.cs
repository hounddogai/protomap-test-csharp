namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual training due notices by code.</summary>
public interface IManualTrainingDueNoticeStore
{
    ManualTrainingDueNotice? Find(string code);
    void Save(ManualTrainingDueNotice item);
    bool Remove(string code);
    List<ManualTrainingDueNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualTrainingDueNoticeStore(Dictionary<string, ManualTrainingDueNotice> items, List<string> order) : IManualTrainingDueNoticeStore
{
    public Dictionary<string, ManualTrainingDueNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualTrainingDueNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualTrainingDueNotice item)
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

    public List<ManualTrainingDueNotice> List()
    {
        var outValue = new List<ManualTrainingDueNotice>();
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
public sealed class CachedManualTrainingDueNoticeStore(MemoryManualTrainingDueNoticeStore inner, ManualTrainingDueNoticeCache cache, int hits, int misses) : IManualTrainingDueNoticeStore
{
    public MemoryManualTrainingDueNoticeStore Inner { get; set; } = inner;
    public ManualTrainingDueNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualTrainingDueNotice? Find(string code)
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

    public void Save(ManualTrainingDueNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualTrainingDueNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual training due notices. Empty fields match all.</summary>
public sealed record ManualTrainingDueNoticeQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(ManualTrainingDueNotice item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualTrainingDueNotice> Apply(List<ManualTrainingDueNotice> items)
    {
        var matched = Collections.Filter(items, (ManualTrainingDueNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual training due notice store.</summary>
public static class ManualTrainingDueNoticeStore
{
    public static IManualTrainingDueNoticeStore NewMemoryManualTrainingDueNoticeStore()
    {
        return new MemoryManualTrainingDueNoticeStore(
            new Dictionary<string, ManualTrainingDueNotice>(),
            new List<string>());
    }

    public static IManualTrainingDueNoticeStore NewCachedManualTrainingDueNoticeStore(MemoryManualTrainingDueNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedManualTrainingDueNoticeStore(
            inner,
            ManualTrainingDueNoticeCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ManualTrainingDueNotice> FindManualTrainingDueNotices(IManualTrainingDueNoticeStore store, ManualTrainingDueNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
