namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pulse survey requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record PulseSurveyRequestManager(IPulseSurveyRequestStore Store, PulseSurveyRequestValidator Validator, IClock Clock, List<string> Journal, PulseSurveyRequestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PulseSurveyRequestStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PulseSurveyRequestStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey request {code} not found");
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

/// <summary>Functions of pulse survey request workflow.</summary>
public static class PulseSurveyRequestWorkflow
{
    public static PulseSurveyRequestManager NewPulseSurveyRequestManager(IPulseSurveyRequestStore store, IClock clock)
    {
        return new PulseSurveyRequestManager(
            store,
            PulseSurveyRequestRules.DefaultPulseSurveyRequestValidator(),
            clock,
            new List<string>(),
            PulseSurveyRequestEngine.DefaultPulseSurveyRequestEngine());
    }
}
