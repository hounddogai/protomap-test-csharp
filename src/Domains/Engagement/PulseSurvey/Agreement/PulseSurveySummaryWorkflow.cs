namespace Acme.Workforce.Engagement.PulseSurvey.Agreement;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pulse survey summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record PulseSurveySummaryService(IPulseSurveySummaryStore Store, PulseSurveySummaryValidator Validator, IClock Clock, List<string> Journal, PulseSurveySummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PulseSurveySummaryStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PulseSurveySummaryStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey summary {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of pulse survey summary workflow.</summary>
public static class PulseSurveySummaryWorkflow
{
    public static PulseSurveySummaryService NewPulseSurveySummaryService(IPulseSurveySummaryStore store, IClock clock)
    {
        return new PulseSurveySummaryService(
            store,
            PulseSurveySummaryRules.DefaultPulseSurveySummaryValidator(),
            clock,
            new List<string>(),
            PulseSurveySummaryEngine.DefaultPulseSurveySummaryEngine());
    }
}
