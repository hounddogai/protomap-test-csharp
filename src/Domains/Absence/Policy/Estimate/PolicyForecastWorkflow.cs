namespace Acme.Workforce.Absence.Policy.Estimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves policy forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyForecastWorkflow(IPolicyForecastStore Store, PolicyForecastValidator Validator, IClock Clock, List<string> Journal, PolicyForecastEngine Engine)
{
    public static PolicyForecastWorkflow NewPolicyForecastWorkflow(IPolicyForecastStore store, IClock clock)
    {
        return new PolicyForecastWorkflow(
            store,
            PolicyForecastRules.DefaultPolicyForecastValidator(),
            clock,
            new List<string>(),
            PolicyForecastEngine.DefaultPolicyForecastEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyForecastStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyForecastStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy forecast {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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
