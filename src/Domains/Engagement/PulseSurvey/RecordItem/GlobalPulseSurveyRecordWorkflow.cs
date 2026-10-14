namespace Acme.Workforce.Engagement.PulseSurvey.RecordItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global pulse survey records through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record GlobalPulseSurveyRecordService(IGlobalPulseSurveyRecordStore Store, GlobalPulseSurveyRecordValidator Validator, IClock Clock, List<string> Journal, GlobalPulseSurveyRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global pulse survey record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalPulseSurveyRecordStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global pulse survey record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalPulseSurveyRecordStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global pulse survey record {code} not found");
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

/// <summary>Functions of global pulse survey record workflow.</summary>
public static class GlobalPulseSurveyRecordWorkflow
{
    public static GlobalPulseSurveyRecordService NewGlobalPulseSurveyRecordService(IGlobalPulseSurveyRecordStore store, IClock clock)
    {
        return new GlobalPulseSurveyRecordService(
            store,
            GlobalPulseSurveyRecordRules.DefaultGlobalPulseSurveyRecordValidator(),
            clock,
            new List<string>(),
            GlobalPulseSurveyRecordEngine.DefaultGlobalPulseSurveyRecordEngine());
    }
}
