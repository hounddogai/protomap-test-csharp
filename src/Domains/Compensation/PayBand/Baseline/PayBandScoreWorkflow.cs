namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pay band scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayBandScoreService(IPayBandScoreStore Store, PayBandScoreValidator Validator, IClock Clock, List<string> Journal, PayBandScoreEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayBandScoreStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayBandScoreStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band score {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of pay band score workflow.</summary>
public static class PayBandScoreWorkflow
{
    public static PayBandScoreService NewPayBandScoreService(IPayBandScoreStore store, IClock clock)
    {
        return new PayBandScoreService(
            store,
            PayBandScoreRules.DefaultPayBandScoreValidator(),
            clock,
            new List<string>(),
            PayBandScoreEngine.DefaultPayBandScoreEngine());
    }
}
