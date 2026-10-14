namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pulse survey assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record PulseSurveyAssignmentDesk(IPulseSurveyAssignmentStore Store, PulseSurveyAssignmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PulseSurveyAssignmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PulseSurveyAssignmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of pulse survey assignment workflow.</summary>
public static class PulseSurveyAssignmentWorkflow
{
    public static PulseSurveyAssignmentDesk NewPulseSurveyAssignmentDesk(IPulseSurveyAssignmentStore store, IClock clock)
    {
        return new PulseSurveyAssignmentDesk(
            store,
            PulseSurveyAssignmentRules.DefaultPulseSurveyAssignmentValidator(),
            clock,
            new List<string>());
    }
}
