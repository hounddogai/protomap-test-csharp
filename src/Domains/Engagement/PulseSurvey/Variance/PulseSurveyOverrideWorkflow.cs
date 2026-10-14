namespace Acme.Workforce.Engagement.PulseSurvey.Variance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pulse survey overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record PulseSurveyOverrideDesk(IPulseSurveyOverrideStore Store, PulseSurveyOverrideValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PulseSurveyOverrideStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PulseSurveyOverrideStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of pulse survey override workflow.</summary>
public static class PulseSurveyOverrideWorkflow
{
    public static PulseSurveyOverrideDesk NewPulseSurveyOverrideDesk(IPulseSurveyOverrideStore store, IClock clock)
    {
        return new PulseSurveyOverrideDesk(
            store,
            PulseSurveyOverrideRules.DefaultPulseSurveyOverrideValidator(),
            clock,
            new List<string>());
    }
}
