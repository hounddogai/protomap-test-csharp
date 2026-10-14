namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient statement of work digests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientStatementOfWorkDigestCoordinator(ILenientStatementOfWorkDigestStore Store, LenientStatementOfWorkDigestValidator Validator, IClock Clock, List<string> Journal, LenientStatementOfWorkDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient statement of work digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientStatementOfWorkDigestStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient statement of work digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientStatementOfWorkDigestStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient statement of work digest {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of lenient statement of work digest workflow.</summary>
public static class LenientStatementOfWorkDigestWorkflow
{
    public static LenientStatementOfWorkDigestCoordinator NewLenientStatementOfWorkDigestCoordinator(ILenientStatementOfWorkDigestStore store, IClock clock, Registry flags)
    {
        return new LenientStatementOfWorkDigestCoordinator(
            store,
            LenientStatementOfWorkDigestRules.DefaultLenientStatementOfWorkDigestValidator(),
            clock,
            new List<string>(),
            LenientStatementOfWorkDigestEngine.DefaultLenientStatementOfWorkDigestEngine(flags));
    }
}
