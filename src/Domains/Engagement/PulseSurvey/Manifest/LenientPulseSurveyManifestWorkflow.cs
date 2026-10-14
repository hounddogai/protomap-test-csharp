namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient pulse survey manifests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientPulseSurveyManifestCoordinator(ILenientPulseSurveyManifestStore Store, LenientPulseSurveyManifestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient pulse survey manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientPulseSurveyManifestStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient pulse survey manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientPulseSurveyManifestStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of lenient pulse survey manifest workflow.</summary>
public static class LenientPulseSurveyManifestWorkflow
{
    public static LenientPulseSurveyManifestCoordinator NewLenientPulseSurveyManifestCoordinator(ILenientPulseSurveyManifestStore store, IClock clock)
    {
        return new LenientPulseSurveyManifestCoordinator(
            store,
            LenientPulseSurveyManifestRules.DefaultLenientPulseSurveyManifestValidator(),
            clock,
            new List<string>());
    }
}
