namespace Acme.Workforce.Compensation.EquityGrant.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending equity grant agreements through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingEquityGrantAgreementWorkflow(IPendingEquityGrantAgreementStore Store, PendingEquityGrantAgreementValidator Validator, IClock Clock, List<string> Journal, PendingEquityGrantAgreementEngine Engine)
{
    public static PendingEquityGrantAgreementWorkflow NewPendingEquityGrantAgreementWorkflow(IPendingEquityGrantAgreementStore store, IClock clock, Registry flags)
    {
        return new PendingEquityGrantAgreementWorkflow(
            store,
            PendingEquityGrantAgreementRules.DefaultPendingEquityGrantAgreementValidator(),
            clock,
            new List<string>(),
            PendingEquityGrantAgreementEngine.DefaultPendingEquityGrantAgreementEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending equity grant agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingEquityGrantAgreementStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending equity grant agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingEquityGrantAgreementStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending equity grant agreement {code} not found");
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
