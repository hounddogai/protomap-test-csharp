namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves rolling sentiment reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingSentimentReservationWorkflow(IRollingSentimentReservationStore Store, RollingSentimentReservationValidator Validator, IClock Clock, List<string> Journal, RollingSentimentReservationEngine Engine)
{
    public static RollingSentimentReservationWorkflow NewRollingSentimentReservationWorkflow(IRollingSentimentReservationStore store, IClock clock, Registry flags)
    {
        return new RollingSentimentReservationWorkflow(
            store,
            RollingSentimentReservationRules.DefaultRollingSentimentReservationValidator(),
            clock,
            new List<string>(),
            RollingSentimentReservationEngine.DefaultRollingSentimentReservationEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling sentiment reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingSentimentReservationStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling sentiment reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingSentimentReservationStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling sentiment reservation {code} not found");
        }
        var result = Engine.Compute(item);
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
