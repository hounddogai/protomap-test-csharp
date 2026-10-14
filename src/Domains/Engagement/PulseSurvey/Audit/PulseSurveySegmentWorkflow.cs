namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pulse survey segments through their life cycle and keeps a journal of what it did.</summary>
public sealed record PulseSurveySegmentWorkflow(IPulseSurveySegmentStore Store, PulseSurveySegmentValidator Validator, IClock Clock, List<string> Journal, PulseSurveySegmentEngine Engine)
{
    public static PulseSurveySegmentWorkflow NewPulseSurveySegmentWorkflow(IPulseSurveySegmentStore store, IClock clock)
    {
        return new PulseSurveySegmentWorkflow(
            store,
            PulseSurveySegmentRules.DefaultPulseSurveySegmentValidator(),
            clock,
            new List<string>(),
            PulseSurveySegmentEngine.DefaultPulseSurveySegmentEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PulseSurveySegmentStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PulseSurveySegmentStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey segment {code} not found");
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
