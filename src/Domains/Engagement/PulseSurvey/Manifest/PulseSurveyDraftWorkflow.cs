namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pulse survey drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record PulseSurveyDraftWorkflow(IPulseSurveyDraftStore Store, PulseSurveyDraftValidator Validator, IClock Clock, List<string> Journal)
{
    public static PulseSurveyDraftWorkflow NewPulseSurveyDraftWorkflow(IPulseSurveyDraftStore store, IClock clock)
    {
        return new PulseSurveyDraftWorkflow(
            store,
            PulseSurveyDraftRules.DefaultPulseSurveyDraftValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PulseSurveyDraftStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PulseSurveyDraftStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
