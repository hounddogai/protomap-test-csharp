namespace Acme.Workforce.Talent.CareerPath.PathDigest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves career path estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record CareerPathEstimateManager(ICareerPathEstimateStore Store, CareerPathEstimateValidator Validator, IClock Clock, List<string> Journal, CareerPathEstimateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CareerPathEstimateStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CareerPathEstimateStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path estimate {code} not found");
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

/// <summary>Functions of career path estimate workflow.</summary>
public static class CareerPathEstimateWorkflow
{
    public static CareerPathEstimateManager NewCareerPathEstimateManager(ICareerPathEstimateStore store, IClock clock)
    {
        return new CareerPathEstimateManager(
            store,
            CareerPathEstimateRules.DefaultCareerPathEstimateValidator(),
            clock,
            new List<string>(),
            CareerPathEstimateEngine.DefaultCareerPathEstimateEngine());
    }
}
