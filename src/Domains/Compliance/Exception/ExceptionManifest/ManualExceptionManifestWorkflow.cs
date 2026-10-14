namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves manual exception manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualExceptionManifestService(IManualExceptionManifestStore Store, ManualExceptionManifestValidator Validator, IClock Clock, List<string> Journal, ManualExceptionManifestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual exception manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualExceptionManifestStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual exception manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualExceptionManifestStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual exception manifest {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of manual exception manifest workflow.</summary>
public static class ManualExceptionManifestWorkflow
{
    public static ManualExceptionManifestService NewManualExceptionManifestService(IManualExceptionManifestStore store, IClock clock, Registry flags)
    {
        return new ManualExceptionManifestService(
            store,
            ManualExceptionManifestRules.DefaultManualExceptionManifestValidator(),
            clock,
            new List<string>(),
            ManualExceptionManifestEngine.DefaultManualExceptionManifestEngine(flags));
    }
}
