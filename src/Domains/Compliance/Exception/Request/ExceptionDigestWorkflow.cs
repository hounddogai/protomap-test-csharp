namespace Acme.Workforce.Compliance.Exception.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves exception digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record ExceptionDigestManager(IExceptionDigestStore Store, ExceptionDigestValidator Validator, IClock Clock, List<string> Journal, ExceptionDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ExceptionDigestStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ExceptionDigestStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception digest {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of exception digest workflow.</summary>
public static class ExceptionDigestWorkflow
{
    public static ExceptionDigestManager NewExceptionDigestManager(IExceptionDigestStore store, IClock clock, Registry flags)
    {
        return new ExceptionDigestManager(
            store,
            ExceptionDigestRules.DefaultExceptionDigestValidator(),
            clock,
            new List<string>(),
            ExceptionDigestEngine.DefaultExceptionDigestEngine(flags));
    }
}
