namespace Acme.Workforce.Compliance.Attestation.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation notices by code.</summary>
public interface IAttestationNoticeStore
{
    AttestationNotice? Find(string code);
    void Save(AttestationNotice item);
    bool Remove(string code);
    List<AttestationNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationNoticeStore(Dictionary<string, AttestationNotice> items, List<string> order) : IAttestationNoticeStore
{
    public Dictionary<string, AttestationNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationNotice item)
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

    public List<AttestationNotice> List()
    {
        var outValue = new List<AttestationNotice>();
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
public sealed class CachedAttestationNoticeStore(MemoryAttestationNoticeStore inner, AttestationNoticeCache cache, int hits, int misses) : IAttestationNoticeStore
{
    public MemoryAttestationNoticeStore Inner { get; set; } = inner;
    public AttestationNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationNotice? Find(string code)
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

    public void Save(AttestationNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation notices. Empty fields match all.</summary>
public sealed record AttestationNoticeQuery(string Title, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationNotice item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationNotice> Apply(List<AttestationNotice> items)
    {
        var matched = Collections.Filter(items, (AttestationNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation notice store.</summary>
public static class AttestationNoticeStore
{
    public static IAttestationNoticeStore NewMemoryAttestationNoticeStore()
    {
        return new MemoryAttestationNoticeStore(new Dictionary<string, AttestationNotice>(), new List<string>());
    }

    public static IAttestationNoticeStore NewCachedAttestationNoticeStore(MemoryAttestationNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationNoticeStore(inner, AttestationNoticeCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AttestationNotice> FindAttestationNotices(IAttestationNoticeStore store, AttestationNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
