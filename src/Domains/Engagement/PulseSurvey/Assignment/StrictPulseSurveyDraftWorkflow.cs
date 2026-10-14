namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves strict pulse survey drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictPulseSurveyDraftCoordinator(IStrictPulseSurveyDraftStore Store, StrictPulseSurveyDraftValidator Validator, IClock Clock, List<string> Journal, StrictPulseSurveyDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict pulse survey draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictPulseSurveyDraftStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict pulse survey draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictPulseSurveyDraftStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict pulse survey draft {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of strict pulse survey draft workflow.</summary>
public static class StrictPulseSurveyDraftWorkflow
{
    public static StrictPulseSurveyDraftCoordinator NewStrictPulseSurveyDraftCoordinator(IStrictPulseSurveyDraftStore store, IClock clock, Registry flags)
    {
        return new StrictPulseSurveyDraftCoordinator(
            store,
            StrictPulseSurveyDraftRules.DefaultStrictPulseSurveyDraftValidator(),
            clock,
            new List<string>(),
            StrictPulseSurveyDraftEngine.DefaultStrictPulseSurveyDraftEngine(flags));
    }
}
