namespace Acme.Workforce.Contractors.Vendor.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves vendor reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record VendorReviewDesk(IVendorReviewStore Store, VendorReviewValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VendorReviewStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VendorReviewStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public int ReviewStale(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.CreatedAt > cutoff)
            {
                continue;
            }
            var problems = Validator.Validate(item);
            if (problems.Count == 0)
            {
                continue;
            }
            Note($"{item.Code} needs attention: {string.Join(", ", problems)}");
            changed += 1;
        }
        return changed;
    }
}

/// <summary>Functions of vendor review workflow.</summary>
public static class VendorReviewWorkflow
{
    public static VendorReviewDesk NewVendorReviewDesk(IVendorReviewStore store, IClock clock)
    {
        return new VendorReviewDesk(
            store,
            VendorReviewRules.DefaultVendorReviewValidator(),
            clock,
            new List<string>());
    }
}
