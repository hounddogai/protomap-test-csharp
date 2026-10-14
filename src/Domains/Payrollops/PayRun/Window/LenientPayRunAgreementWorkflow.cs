namespace Acme.Workforce.Payrollops.PayRun.Window;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient pay run agreements through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientPayRunAgreementCoordinator(ILenientPayRunAgreementStore Store, LenientPayRunAgreementValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient pay run agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientPayRunAgreementStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient pay run agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientPayRunAgreementStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of lenient pay run agreement workflow.</summary>
public static class LenientPayRunAgreementWorkflow
{
    public static LenientPayRunAgreementCoordinator NewLenientPayRunAgreementCoordinator(ILenientPayRunAgreementStore store, IClock clock)
    {
        return new LenientPayRunAgreementCoordinator(
            store,
            LenientPayRunAgreementRules.DefaultLenientPayRunAgreementValidator(),
            clock,
            new List<string>());
    }
}
