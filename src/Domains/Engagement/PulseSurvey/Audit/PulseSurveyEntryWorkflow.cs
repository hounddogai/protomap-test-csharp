namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pulse survey entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record PulseSurveyEntryCoordinator(IPulseSurveyEntryStore Store, PulseSurveyEntryValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PulseSurveyEntryStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PulseSurveyEntryStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of pulse survey entry workflow.</summary>
public static class PulseSurveyEntryWorkflow
{
    public static PulseSurveyEntryCoordinator NewPulseSurveyEntryCoordinator(IPulseSurveyEntryStore store, IClock clock)
    {
        return new PulseSurveyEntryCoordinator(
            store,
            PulseSurveyEntryRules.DefaultPulseSurveyEntryValidator(),
            clock,
            new List<string>());
    }
}
