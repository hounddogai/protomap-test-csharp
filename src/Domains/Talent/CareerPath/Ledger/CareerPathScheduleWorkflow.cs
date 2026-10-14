namespace Acme.Workforce.Talent.CareerPath.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves career path schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record CareerPathScheduleCoordinator(ICareerPathScheduleStore Store, CareerPathScheduleValidator Validator, IClock Clock, List<string> Journal, CareerPathScheduleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CareerPathScheduleStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CareerPathScheduleStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path schedule {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of career path schedule workflow.</summary>
public static class CareerPathScheduleWorkflow
{
    public static CareerPathScheduleCoordinator NewCareerPathScheduleCoordinator(ICareerPathScheduleStore store, IClock clock)
    {
        return new CareerPathScheduleCoordinator(
            store,
            CareerPathScheduleRules.DefaultCareerPathScheduleValidator(),
            clock,
            new List<string>(),
            CareerPathScheduleEngine.DefaultCareerPathScheduleEngine());
    }
}
