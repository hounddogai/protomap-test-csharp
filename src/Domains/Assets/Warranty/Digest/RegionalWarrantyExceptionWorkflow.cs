namespace Acme.Workforce.Assets.Warranty.Digest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional warranty exceptions through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalWarrantyExceptionCoordinator(IRegionalWarrantyExceptionStore Store, RegionalWarrantyExceptionValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional warranty exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalWarrantyExceptionStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional warranty exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalWarrantyExceptionStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public int Sweep(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
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

/// <summary>Functions of regional warranty exception workflow.</summary>
public static class RegionalWarrantyExceptionWorkflow
{
    public static RegionalWarrantyExceptionCoordinator NewRegionalWarrantyExceptionCoordinator(IRegionalWarrantyExceptionStore store, IClock clock)
    {
        return new RegionalWarrantyExceptionCoordinator(
            store,
            RegionalWarrantyExceptionRules.DefaultRegionalWarrantyExceptionValidator(),
            clock,
            new List<string>());
    }
}
