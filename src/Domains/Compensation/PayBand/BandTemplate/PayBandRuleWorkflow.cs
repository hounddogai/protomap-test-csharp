namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pay band rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayBandRuleCoordinator(IPayBandRuleStore Store, PayBandRuleValidator Validator, IClock Clock, List<string> Journal, PayBandRuleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayBandRuleStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayBandRuleStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band rule {code} not found");
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

/// <summary>Functions of pay band rule workflow.</summary>
public static class PayBandRuleWorkflow
{
    public static PayBandRuleCoordinator NewPayBandRuleCoordinator(IPayBandRuleStore store, IClock clock)
    {
        return new PayBandRuleCoordinator(
            store,
            PayBandRuleRules.DefaultPayBandRuleValidator(),
            clock,
            new List<string>(),
            PayBandRuleEngine.DefaultPayBandRuleEngine());
    }
}
