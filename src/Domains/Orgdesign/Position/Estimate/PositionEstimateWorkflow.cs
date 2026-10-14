namespace Acme.Workforce.Orgdesign.Position.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves position estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionEstimateService(IPositionEstimateStore Store, PositionEstimateValidator Validator, IClock Clock, List<string> Journal, PositionEstimateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionEstimateStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionEstimateStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position estimate {code} not found");
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

/// <summary>Functions of position estimate workflow.</summary>
public static class PositionEstimateWorkflow
{
    public static PositionEstimateService NewPositionEstimateService(IPositionEstimateStore store, IClock clock, Registry flags)
    {
        return new PositionEstimateService(
            store,
            PositionEstimateRules.DefaultPositionEstimateValidator(),
            clock,
            new List<string>(),
            PositionEstimateEngine.DefaultPositionEstimateEngine(flags));
    }
}
