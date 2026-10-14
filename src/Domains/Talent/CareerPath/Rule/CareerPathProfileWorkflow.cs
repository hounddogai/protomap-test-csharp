namespace Acme.Workforce.Talent.CareerPath.Rule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves career path profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record CareerPathProfileCoordinator(ICareerPathProfileStore Store, CareerPathProfileValidator Validator, IClock Clock, List<string> Journal, CareerPathProfileEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CareerPathProfileStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CareerPathProfileStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path profile {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of career path profile workflow.</summary>
public static class CareerPathProfileWorkflow
{
    public static CareerPathProfileCoordinator NewCareerPathProfileCoordinator(ICareerPathProfileStore store, IClock clock)
    {
        return new CareerPathProfileCoordinator(
            store,
            CareerPathProfileRules.DefaultCareerPathProfileValidator(),
            clock,
            new List<string>(),
            CareerPathProfileEngine.DefaultCareerPathProfileEngine());
    }
}
