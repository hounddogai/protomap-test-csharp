namespace Acme.Workforce.Engagement.Recognition.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves recognition reservations through their life cycle and keeps a journal of what it did.</summary>
public sealed record RecognitionReservationCoordinator(IRecognitionReservationStore Store, RecognitionReservationValidator Validator, IClock Clock, List<string> Journal, RecognitionReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RecognitionReservationStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RecognitionReservationStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition reservation {code} not found");
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

/// <summary>Functions of recognition reservation workflow.</summary>
public static class RecognitionReservationWorkflow
{
    public static RecognitionReservationCoordinator NewRecognitionReservationCoordinator(IRecognitionReservationStore store, IClock clock, Registry flags)
    {
        return new RecognitionReservationCoordinator(
            store,
            RecognitionReservationRules.DefaultRecognitionReservationValidator(),
            clock,
            new List<string>(),
            RecognitionReservationEngine.DefaultRecognitionReservationEngine(flags));
    }
}
