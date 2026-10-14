namespace Acme.Workforce.Talent.SkillProfile.Forecast;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched skill profile forecasts through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedSkillProfileForecastDesk(IBatchedSkillProfileForecastStore Store, BatchedSkillProfileForecastValidator Validator, IClock Clock, List<string> Journal, BatchedSkillProfileForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched skill profile forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedSkillProfileForecastStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched skill profile forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedSkillProfileForecastStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched skill profile forecast {code} not found");
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

/// <summary>Functions of batched skill profile forecast workflow.</summary>
public static class BatchedSkillProfileForecastWorkflow
{
    public static BatchedSkillProfileForecastDesk NewBatchedSkillProfileForecastDesk(IBatchedSkillProfileForecastStore store, IClock clock)
    {
        return new BatchedSkillProfileForecastDesk(
            store,
            BatchedSkillProfileForecastRules.DefaultBatchedSkillProfileForecastValidator(),
            clock,
            new List<string>(),
            BatchedSkillProfileForecastEngine.DefaultBatchedSkillProfileForecastEngine());
    }
}
