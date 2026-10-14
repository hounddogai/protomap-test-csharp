namespace Acme.Workforce.Engagement.Recognition.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition agreements by code.</summary>
public interface IRecognitionAgreementStore
{
    RecognitionAgreement? Find(string code);
    void Save(RecognitionAgreement item);
    bool Remove(string code);
    List<RecognitionAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionAgreementStore(Dictionary<string, RecognitionAgreement> items, List<string> order) : IRecognitionAgreementStore
{
    public Dictionary<string, RecognitionAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionAgreement item)
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

    public List<RecognitionAgreement> List()
    {
        var outValue = new List<RecognitionAgreement>();
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
public sealed class CachedRecognitionAgreementStore(MemoryRecognitionAgreementStore inner, RecognitionAgreementCache cache, int hits, int misses) : IRecognitionAgreementStore
{
    public MemoryRecognitionAgreementStore Inner { get; set; } = inner;
    public RecognitionAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionAgreement? Find(string code)
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

    public void Save(RecognitionAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition agreements. Empty fields match all.</summary>
public sealed record RecognitionAgreementQuery(string Locale, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionAgreement item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RecognitionAgreement> Apply(List<RecognitionAgreement> items)
    {
        var matched = Collections.Filter(items, (RecognitionAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition agreement store.</summary>
public static class RecognitionAgreementStore
{
    public static IRecognitionAgreementStore NewMemoryRecognitionAgreementStore()
    {
        return new MemoryRecognitionAgreementStore(
            new Dictionary<string, RecognitionAgreement>(),
            new List<string>());
    }

    public static IRecognitionAgreementStore NewCachedRecognitionAgreementStore(MemoryRecognitionAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionAgreementStore(inner, RecognitionAgreementCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RecognitionAgreement> FindRecognitionAgreements(IRecognitionAgreementStore store, RecognitionAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
