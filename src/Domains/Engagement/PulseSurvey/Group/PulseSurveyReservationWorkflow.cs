namespace Acme.Workforce.Engagement.PulseSurvey.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pulse survey reservations through their life cycle and keeps a journal of what it did.</summary>
public sealed record PulseSurveyReservationWorkflow(IPulseSurveyReservationStore Store, PulseSurveyReservationValidator Validator, IClock Clock, List<string> Journal, PulseSurveyReservationEngine Engine)
{
    public static PulseSurveyReservationWorkflow NewPulseSurveyReservationWorkflow(IPulseSurveyReservationStore store, IClock clock, Registry flags)
    {
        return new PulseSurveyReservationWorkflow(
            store,
            PulseSurveyReservationRules.DefaultPulseSurveyReservationValidator(),
            clock,
            new List<string>(),
            PulseSurveyReservationEngine.DefaultPulseSurveyReservationEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PulseSurveyReservationStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PulseSurveyReservationStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey reservation {code} not found");
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
