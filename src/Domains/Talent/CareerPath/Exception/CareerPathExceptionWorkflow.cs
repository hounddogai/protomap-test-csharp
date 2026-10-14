namespace Acme.Workforce.Talent.CareerPath.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves career path exceptions through their life cycle and keeps a journal of what it did.</summary>
public sealed record CareerPathExceptionDesk(ICareerPathExceptionStore Store, CareerPathExceptionValidator Validator, IClock Clock, List<string> Journal, CareerPathExceptionEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CareerPathExceptionStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CareerPathExceptionStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path exception {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of career path exception workflow.</summary>
public static class CareerPathExceptionWorkflow
{
    public static CareerPathExceptionDesk NewCareerPathExceptionDesk(ICareerPathExceptionStore store, IClock clock, Registry flags)
    {
        return new CareerPathExceptionDesk(
            store,
            CareerPathExceptionRules.DefaultCareerPathExceptionValidator(),
            clock,
            new List<string>(),
            CareerPathExceptionEngine.DefaultCareerPathExceptionEngine(flags));
    }
}
