namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy statement of work agreements through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LegacyStatementOfWorkAgreementManager(ILegacyStatementOfWorkAgreementStore Store, LegacyStatementOfWorkAgreementValidator Validator, IClock Clock, List<string> Journal, LegacyStatementOfWorkAgreementEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy statement of work agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyStatementOfWorkAgreementStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy statement of work agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyStatementOfWorkAgreementStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy statement of work agreement {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of legacy statement of work agreement workflow.</summary>
public static class LegacyStatementOfWorkAgreementWorkflow
{
    public static LegacyStatementOfWorkAgreementManager NewLegacyStatementOfWorkAgreementManager(ILegacyStatementOfWorkAgreementStore store, IClock clock)
    {
        return new LegacyStatementOfWorkAgreementManager(
            store,
            LegacyStatementOfWorkAgreementRules.DefaultLegacyStatementOfWorkAgreementValidator(),
            clock,
            new List<string>(),
            LegacyStatementOfWorkAgreementEngine.DefaultLegacyStatementOfWorkAgreementEngine());
    }
}
