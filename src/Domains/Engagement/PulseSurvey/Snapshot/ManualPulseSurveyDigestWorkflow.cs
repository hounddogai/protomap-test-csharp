namespace Acme.Workforce.Engagement.PulseSurvey.Snapshot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual pulse survey digests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ManualPulseSurveyDigestDesk(IManualPulseSurveyDigestStore Store, ManualPulseSurveyDigestValidator Validator, IClock Clock, List<string> Journal, ManualPulseSurveyDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual pulse survey digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualPulseSurveyDigestStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual pulse survey digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualPulseSurveyDigestStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual pulse survey digest {code} not found");
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

/// <summary>Functions of manual pulse survey digest workflow.</summary>
public static class ManualPulseSurveyDigestWorkflow
{
    public static ManualPulseSurveyDigestDesk NewManualPulseSurveyDigestDesk(IManualPulseSurveyDigestStore store, IClock clock)
    {
        return new ManualPulseSurveyDigestDesk(
            store,
            ManualPulseSurveyDigestRules.DefaultManualPulseSurveyDigestValidator(),
            clock,
            new List<string>(),
            ManualPulseSurveyDigestEngine.DefaultManualPulseSurveyDigestEngine());
    }
}
