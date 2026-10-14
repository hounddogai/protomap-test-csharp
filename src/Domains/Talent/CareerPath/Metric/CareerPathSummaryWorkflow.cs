namespace Acme.Workforce.Talent.CareerPath.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves career path summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record CareerPathSummaryCoordinator(ICareerPathSummaryStore Store, CareerPathSummaryValidator Validator, IClock Clock, List<string> Journal, CareerPathSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CareerPathSummaryStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CareerPathSummaryStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path summary {code} not found");
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

/// <summary>Functions of career path summary workflow.</summary>
public static class CareerPathSummaryWorkflow
{
    public static CareerPathSummaryCoordinator NewCareerPathSummaryCoordinator(ICareerPathSummaryStore store, IClock clock, Registry flags)
    {
        return new CareerPathSummaryCoordinator(
            store,
            CareerPathSummaryRules.DefaultCareerPathSummaryValidator(),
            clock,
            new List<string>(),
            CareerPathSummaryEngine.DefaultCareerPathSummaryEngine(flags));
    }
}
