namespace Acme.Workforce.Compliance.Exception.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves exception manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record ExceptionManifestManager(IExceptionManifestStore Store, ExceptionManifestValidator Validator, IClock Clock, List<string> Journal, ExceptionManifestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ExceptionManifestStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ExceptionManifestStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception manifest {code} not found");
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

/// <summary>Functions of exception manifest workflow.</summary>
public static class ExceptionManifestWorkflow
{
    public static ExceptionManifestManager NewExceptionManifestManager(IExceptionManifestStore store, IClock clock, Registry flags)
    {
        return new ExceptionManifestManager(
            store,
            ExceptionManifestRules.DefaultExceptionManifestValidator(),
            clock,
            new List<string>(),
            ExceptionManifestEngine.DefaultExceptionManifestEngine(flags));
    }
}
