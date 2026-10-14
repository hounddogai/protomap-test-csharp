namespace Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves seasonal feedback estimates through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalFeedbackEstimateDesk(ISeasonalFeedbackEstimateStore Store, SeasonalFeedbackEstimateValidator Validator, IClock Clock, List<string> Journal, SeasonalFeedbackEstimateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal feedback estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalFeedbackEstimateStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal feedback estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalFeedbackEstimateStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal feedback estimate {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of seasonal feedback estimate workflow.</summary>
public static class SeasonalFeedbackEstimateWorkflow
{
    public static SeasonalFeedbackEstimateDesk NewSeasonalFeedbackEstimateDesk(ISeasonalFeedbackEstimateStore store, IClock clock, Registry flags)
    {
        return new SeasonalFeedbackEstimateDesk(
            store,
            SeasonalFeedbackEstimateRules.DefaultSeasonalFeedbackEstimateValidator(),
            clock,
            new List<string>(),
            SeasonalFeedbackEstimateEngine.DefaultSeasonalFeedbackEstimateEngine(flags));
    }
}
