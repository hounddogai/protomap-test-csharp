namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pulse survey reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record PulseSurveyReviewManager(IPulseSurveyReviewStore Store, PulseSurveyReviewValidator Validator, IClock Clock, List<string> Journal, PulseSurveyReviewEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PulseSurveyReviewStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PulseSurveyReviewStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey review {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of pulse survey review workflow.</summary>
public static class PulseSurveyReviewWorkflow
{
    public static PulseSurveyReviewManager NewPulseSurveyReviewManager(IPulseSurveyReviewStore store, IClock clock, Registry flags)
    {
        return new PulseSurveyReviewManager(
            store,
            PulseSurveyReviewRules.DefaultPulseSurveyReviewValidator(),
            clock,
            new List<string>(),
            PulseSurveyReviewEngine.DefaultPulseSurveyReviewEngine(flags));
    }
}
