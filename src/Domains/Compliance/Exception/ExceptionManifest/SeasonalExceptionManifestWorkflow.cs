namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal exception manifests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalExceptionManifestWorkflow(ISeasonalExceptionManifestStore Store, SeasonalExceptionManifestValidator Validator, IClock Clock, List<string> Journal, SeasonalExceptionManifestEngine Engine)
{
    public static SeasonalExceptionManifestWorkflow NewSeasonalExceptionManifestWorkflow(ISeasonalExceptionManifestStore store, IClock clock)
    {
        return new SeasonalExceptionManifestWorkflow(
            store,
            SeasonalExceptionManifestRules.DefaultSeasonalExceptionManifestValidator(),
            clock,
            new List<string>(),
            SeasonalExceptionManifestEngine.DefaultSeasonalExceptionManifestEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal exception manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalExceptionManifestStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal exception manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalExceptionManifestStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal exception manifest {code} not found");
        }
        var result = Engine.Evaluate(item);
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
