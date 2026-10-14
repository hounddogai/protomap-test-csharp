namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient leave request agreements through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientLeaveRequestAgreementService(ILenientLeaveRequestAgreementStore Store, LenientLeaveRequestAgreementValidator Validator, IClock Clock, List<string> Journal, LenientLeaveRequestAgreementEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient leave request agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientLeaveRequestAgreementStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient leave request agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientLeaveRequestAgreementStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient leave request agreement {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of lenient leave request agreement workflow.</summary>
public static class LenientLeaveRequestAgreementWorkflow
{
    public static LenientLeaveRequestAgreementService NewLenientLeaveRequestAgreementService(ILenientLeaveRequestAgreementStore store, IClock clock, Registry flags)
    {
        return new LenientLeaveRequestAgreementService(
            store,
            LenientLeaveRequestAgreementRules.DefaultLenientLeaveRequestAgreementValidator(),
            clock,
            new List<string>(),
            LenientLeaveRequestAgreementEngine.DefaultLenientLeaveRequestAgreementEngine(flags));
    }
}
