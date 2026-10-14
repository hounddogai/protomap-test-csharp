namespace Acme.Workforce.Engagement.PulseSurvey.Tier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pulse survey slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record PulseSurveySlotWorkflow(IPulseSurveySlotStore Store, PulseSurveySlotValidator Validator, IClock Clock, List<string> Journal, PulseSurveySlotEngine Engine)
{
    public static PulseSurveySlotWorkflow NewPulseSurveySlotWorkflow(IPulseSurveySlotStore store, IClock clock)
    {
        return new PulseSurveySlotWorkflow(
            store,
            PulseSurveySlotRules.DefaultPulseSurveySlotValidator(),
            clock,
            new List<string>(),
            PulseSurveySlotEngine.DefaultPulseSurveySlotEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PulseSurveySlotStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PulseSurveySlotStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey slot {code} not found");
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
