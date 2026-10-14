namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves offboarding metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record OffboardingMetricDesk(IOffboardingMetricStore Store, OffboardingMetricValidator Validator, IClock Clock, List<string> Journal, OffboardingMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OffboardingMetricStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OffboardingMetricStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding metric {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of offboarding metric workflow.</summary>
public static class OffboardingMetricWorkflow
{
    public static OffboardingMetricDesk NewOffboardingMetricDesk(IOffboardingMetricStore store, IClock clock)
    {
        return new OffboardingMetricDesk(
            store,
            OffboardingMetricRules.DefaultOffboardingMetricValidator(),
            clock,
            new List<string>(),
            OffboardingMetricEngine.DefaultOffboardingMetricEngine());
    }
}
