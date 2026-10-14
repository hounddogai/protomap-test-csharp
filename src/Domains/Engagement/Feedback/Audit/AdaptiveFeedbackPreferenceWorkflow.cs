namespace Acme.Workforce.Engagement.Feedback.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive feedback preferences through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveFeedbackPreferenceDesk(IAdaptiveFeedbackPreferenceStore Store, AdaptiveFeedbackPreferenceValidator Validator, IClock Clock, List<string> Journal, AdaptiveFeedbackPreferenceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive feedback preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveFeedbackPreferenceStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive feedback preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveFeedbackPreferenceStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive feedback preference {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of adaptive feedback preference workflow.</summary>
public static class AdaptiveFeedbackPreferenceWorkflow
{
    public static AdaptiveFeedbackPreferenceDesk NewAdaptiveFeedbackPreferenceDesk(IAdaptiveFeedbackPreferenceStore store, IClock clock, Registry flags)
    {
        return new AdaptiveFeedbackPreferenceDesk(
            store,
            AdaptiveFeedbackPreferenceRules.DefaultAdaptiveFeedbackPreferenceValidator(),
            clock,
            new List<string>(),
            AdaptiveFeedbackPreferenceEngine.DefaultAdaptiveFeedbackPreferenceEngine(flags));
    }
}
