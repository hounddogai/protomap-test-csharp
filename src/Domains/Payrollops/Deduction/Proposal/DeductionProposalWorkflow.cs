namespace Acme.Workforce.Payrollops.Deduction.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves deduction proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeductionProposalService(IDeductionProposalStore Store, DeductionProposalValidator Validator, IClock Clock, List<string> Journal, DeductionProposalEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeductionProposalStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeductionProposalStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction proposal {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of deduction proposal workflow.</summary>
public static class DeductionProposalWorkflow
{
    public static DeductionProposalService NewDeductionProposalService(IDeductionProposalStore store, IClock clock, Registry flags)
    {
        return new DeductionProposalService(
            store,
            DeductionProposalRules.DefaultDeductionProposalValidator(),
            clock,
            new List<string>(),
            DeductionProposalEngine.DefaultDeductionProposalEngine(flags));
    }
}
