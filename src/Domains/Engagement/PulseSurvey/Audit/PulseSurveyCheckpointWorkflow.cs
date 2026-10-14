namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pulse survey checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record PulseSurveyCheckpointManager(IPulseSurveyCheckpointStore Store, PulseSurveyCheckpointValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PulseSurveyCheckpointStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PulseSurveyCheckpointStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public int Sweep(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of pulse survey checkpoint workflow.</summary>
public static class PulseSurveyCheckpointWorkflow
{
    public static PulseSurveyCheckpointManager NewPulseSurveyCheckpointManager(IPulseSurveyCheckpointStore store, IClock clock)
    {
        return new PulseSurveyCheckpointManager(
            store,
            PulseSurveyCheckpointRules.DefaultPulseSurveyCheckpointValidator(),
            clock,
            new List<string>());
    }
}
