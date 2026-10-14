namespace Acme.Workforce.Engagement.Feedback.FeedbackReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves fallback feedback reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackFeedbackReservationService(IFallbackFeedbackReservationStore Store, FallbackFeedbackReservationValidator Validator, IClock Clock, List<string> Journal, FallbackFeedbackReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback feedback reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackFeedbackReservationStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback feedback reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackFeedbackReservationStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback feedback reservation {code} not found");
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of fallback feedback reservation workflow.</summary>
public static class FallbackFeedbackReservationWorkflow
{
    public static FallbackFeedbackReservationService NewFallbackFeedbackReservationService(IFallbackFeedbackReservationStore store, IClock clock, Registry flags)
    {
        return new FallbackFeedbackReservationService(
            store,
            FallbackFeedbackReservationRules.DefaultFallbackFeedbackReservationValidator(),
            clock,
            new List<string>(),
            FallbackFeedbackReservationEngine.DefaultFallbackFeedbackReservationEngine(flags));
    }
}
