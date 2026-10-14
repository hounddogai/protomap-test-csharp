namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tiered statement of work agreements through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record TieredStatementOfWorkAgreementDesk(ITieredStatementOfWorkAgreementStore Store, TieredStatementOfWorkAgreementValidator Validator, IClock Clock, List<string> Journal, TieredStatementOfWorkAgreementEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered statement of work agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredStatementOfWorkAgreementStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered statement of work agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredStatementOfWorkAgreementStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered statement of work agreement {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of tiered statement of work agreement workflow.</summary>
public static class TieredStatementOfWorkAgreementWorkflow
{
    public static TieredStatementOfWorkAgreementDesk NewTieredStatementOfWorkAgreementDesk(ITieredStatementOfWorkAgreementStore store, IClock clock, Registry flags)
    {
        return new TieredStatementOfWorkAgreementDesk(
            store,
            TieredStatementOfWorkAgreementRules.DefaultTieredStatementOfWorkAgreementValidator(),
            clock,
            new List<string>(),
            TieredStatementOfWorkAgreementEngine.DefaultTieredStatementOfWorkAgreementEngine(flags));
    }
}
