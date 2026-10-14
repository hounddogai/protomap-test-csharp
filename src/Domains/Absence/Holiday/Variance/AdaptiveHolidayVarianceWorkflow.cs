namespace Acme.Workforce.Absence.Holiday.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive holiday variances through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveHolidayVarianceCoordinator(IAdaptiveHolidayVarianceStore Store, AdaptiveHolidayVarianceValidator Validator, IClock Clock, List<string> Journal, AdaptiveHolidayVarianceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive holiday variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveHolidayVarianceStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive holiday variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveHolidayVarianceStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive holiday variance {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of adaptive holiday variance workflow.</summary>
public static class AdaptiveHolidayVarianceWorkflow
{
    public static AdaptiveHolidayVarianceCoordinator NewAdaptiveHolidayVarianceCoordinator(IAdaptiveHolidayVarianceStore store, IClock clock, Registry flags)
    {
        return new AdaptiveHolidayVarianceCoordinator(
            store,
            AdaptiveHolidayVarianceRules.DefaultAdaptiveHolidayVarianceValidator(),
            clock,
            new List<string>(),
            AdaptiveHolidayVarianceEngine.DefaultAdaptiveHolidayVarianceEngine(flags));
    }
}
