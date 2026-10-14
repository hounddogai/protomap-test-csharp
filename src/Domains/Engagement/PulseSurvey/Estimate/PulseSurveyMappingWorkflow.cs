namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pulse survey mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record PulseSurveyMappingDesk(IPulseSurveyMappingStore Store, PulseSurveyMappingValidator Validator, IClock Clock, List<string> Journal, PulseSurveyMappingEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PulseSurveyMappingStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PulseSurveyMappingStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey mapping {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of pulse survey mapping workflow.</summary>
public static class PulseSurveyMappingWorkflow
{
    public static PulseSurveyMappingDesk NewPulseSurveyMappingDesk(IPulseSurveyMappingStore store, IClock clock, Registry flags)
    {
        return new PulseSurveyMappingDesk(
            store,
            PulseSurveyMappingRules.DefaultPulseSurveyMappingValidator(),
            clock,
            new List<string>(),
            PulseSurveyMappingEngine.DefaultPulseSurveyMappingEngine(flags));
    }
}
