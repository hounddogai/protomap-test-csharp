namespace Acme.Workforce.Payrollops.TaxForm.Proposal;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tax form proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record TaxFormProposalManager(ITaxFormProposalStore Store, TaxFormProposalValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TaxFormProposalStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TaxFormProposalStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of tax form proposal workflow.</summary>
public static class TaxFormProposalWorkflow
{
    public static TaxFormProposalManager NewTaxFormProposalManager(ITaxFormProposalStore store, IClock clock)
    {
        return new TaxFormProposalManager(
            store,
            TaxFormProposalRules.DefaultTaxFormProposalValidator(),
            clock,
            new List<string>());
    }
}
