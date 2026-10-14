namespace Acme.Workforce.Engagement.Feedback.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive feedback scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveFeedbackScoreCoordinator(IAdaptiveFeedbackScoreStore Store, AdaptiveFeedbackScoreValidator Validator, IClock Clock, List<string> Journal, AdaptiveFeedbackScoreEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive feedback score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveFeedbackScoreStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive feedback score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveFeedbackScoreStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive feedback score {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of adaptive feedback score workflow.</summary>
public static class AdaptiveFeedbackScoreWorkflow
{
    public static AdaptiveFeedbackScoreCoordinator NewAdaptiveFeedbackScoreCoordinator(IAdaptiveFeedbackScoreStore store, IClock clock, Registry flags)
    {
        return new AdaptiveFeedbackScoreCoordinator(
            store,
            AdaptiveFeedbackScoreRules.DefaultAdaptiveFeedbackScoreValidator(),
            clock,
            new List<string>(),
            AdaptiveFeedbackScoreEngine.DefaultAdaptiveFeedbackScoreEngine(flags));
    }
}
