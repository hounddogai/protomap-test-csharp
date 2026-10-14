namespace Acme.Workforce.Contractors.StatementOfWork.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient statement of work requests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientStatementOfWorkRequestService(ILenientStatementOfWorkRequestStore Store, LenientStatementOfWorkRequestValidator Validator, IClock Clock, List<string> Journal, LenientStatementOfWorkRequestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient statement of work request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientStatementOfWorkRequestStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient statement of work request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientStatementOfWorkRequestStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient statement of work request {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of lenient statement of work request workflow.</summary>
public static class LenientStatementOfWorkRequestWorkflow
{
    public static LenientStatementOfWorkRequestService NewLenientStatementOfWorkRequestService(ILenientStatementOfWorkRequestStore store, IClock clock, Registry flags)
    {
        return new LenientStatementOfWorkRequestService(
            store,
            LenientStatementOfWorkRequestRules.DefaultLenientStatementOfWorkRequestValidator(),
            clock,
            new List<string>(),
            LenientStatementOfWorkRequestEngine.DefaultLenientStatementOfWorkRequestEngine(flags));
    }
}
