namespace Acme.Workforce.Talent.CareerPath.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves career path adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record CareerPathAdjustmentManager(ICareerPathAdjustmentStore Store, CareerPathAdjustmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CareerPathAdjustmentStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CareerPathAdjustmentStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of career path adjustment workflow.</summary>
public static class CareerPathAdjustmentWorkflow
{
    public static CareerPathAdjustmentManager NewCareerPathAdjustmentManager(ICareerPathAdjustmentStore store, IClock clock)
    {
        return new CareerPathAdjustmentManager(
            store,
            CareerPathAdjustmentRules.DefaultCareerPathAdjustmentValidator(),
            clock,
            new List<string>());
    }
}
