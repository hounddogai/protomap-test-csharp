namespace Acme.Workforce.Assets.Warranty.Manifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves warranty proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyProposalCoordinator(IWarrantyProposalStore Store, WarrantyProposalValidator Validator, IClock Clock, List<string> Journal, WarrantyProposalEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyProposalStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyProposalStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty proposal {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of warranty proposal workflow.</summary>
public static class WarrantyProposalWorkflow
{
    public static WarrantyProposalCoordinator NewWarrantyProposalCoordinator(IWarrantyProposalStore store, IClock clock)
    {
        return new WarrantyProposalCoordinator(
            store,
            WarrantyProposalRules.DefaultWarrantyProposalValidator(),
            clock,
            new List<string>(),
            WarrantyProposalEngine.DefaultWarrantyProposalEngine());
    }
}
