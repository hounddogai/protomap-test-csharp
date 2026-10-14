namespace Acme.Workforce.Engagement.Feedback.Notice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered feedback preferences through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record TieredFeedbackPreferenceDesk(ITieredFeedbackPreferenceStore Store, TieredFeedbackPreferenceValidator Validator, IClock Clock, List<string> Journal, TieredFeedbackPreferenceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered feedback preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredFeedbackPreferenceStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered feedback preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredFeedbackPreferenceStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered feedback preference {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of tiered feedback preference workflow.</summary>
public static class TieredFeedbackPreferenceWorkflow
{
    public static TieredFeedbackPreferenceDesk NewTieredFeedbackPreferenceDesk(ITieredFeedbackPreferenceStore store, IClock clock)
    {
        return new TieredFeedbackPreferenceDesk(
            store,
            TieredFeedbackPreferenceRules.DefaultTieredFeedbackPreferenceValidator(),
            clock,
            new List<string>(),
            TieredFeedbackPreferenceEngine.DefaultTieredFeedbackPreferenceEngine());
    }
}
