namespace Acme.Workforce.Engagement.PulseSurvey.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pulse survey rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record PulseSurveyRollupService(IPulseSurveyRollupStore Store, PulseSurveyRollupValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PulseSurveyRollupStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PulseSurveyRollupStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of pulse survey rollup workflow.</summary>
public static class PulseSurveyRollupWorkflow
{
    public static PulseSurveyRollupService NewPulseSurveyRollupService(IPulseSurveyRollupStore store, IClock clock)
    {
        return new PulseSurveyRollupService(
            store,
            PulseSurveyRollupRules.DefaultPulseSurveyRollupValidator(),
            clock,
            new List<string>());
    }
}
