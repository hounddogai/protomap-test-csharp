namespace Acme.Workforce.Compliance.Attestation.AttestationEstimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves strict attestation budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictAttestationBudgetDesk(IStrictAttestationBudgetStore Store, StrictAttestationBudgetValidator Validator, IClock Clock, List<string> Journal, StrictAttestationBudgetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict attestation budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictAttestationBudgetStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict attestation budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictAttestationBudgetStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict attestation budget {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of strict attestation budget workflow.</summary>
public static class StrictAttestationBudgetWorkflow
{
    public static StrictAttestationBudgetDesk NewStrictAttestationBudgetDesk(IStrictAttestationBudgetStore store, IClock clock, Registry flags)
    {
        return new StrictAttestationBudgetDesk(
            store,
            StrictAttestationBudgetRules.DefaultStrictAttestationBudgetValidator(),
            clock,
            new List<string>(),
            StrictAttestationBudgetEngine.DefaultStrictAttestationBudgetEngine(flags));
    }
}
