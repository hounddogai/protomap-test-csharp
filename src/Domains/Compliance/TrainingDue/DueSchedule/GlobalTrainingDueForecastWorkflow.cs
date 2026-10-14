namespace Acme.Workforce.Compliance.TrainingDue.DueSchedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global training due forecasts through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record GlobalTrainingDueForecastWorkflow(IGlobalTrainingDueForecastStore Store, GlobalTrainingDueForecastValidator Validator, IClock Clock, List<string> Journal)
{
    public static GlobalTrainingDueForecastWorkflow NewGlobalTrainingDueForecastWorkflow(IGlobalTrainingDueForecastStore store, IClock clock)
    {
        return new GlobalTrainingDueForecastWorkflow(
            store,
            GlobalTrainingDueForecastRules.DefaultGlobalTrainingDueForecastValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global training due forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalTrainingDueForecastStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global training due forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalTrainingDueForecastStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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
