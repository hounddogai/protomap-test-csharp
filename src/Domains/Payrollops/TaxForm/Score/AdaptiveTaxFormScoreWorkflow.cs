namespace Acme.Workforce.Payrollops.TaxForm.Score;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive tax form scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveTaxFormScoreService(IAdaptiveTaxFormScoreStore Store, AdaptiveTaxFormScoreValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive tax form score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveTaxFormScoreStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive tax form score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveTaxFormScoreStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of adaptive tax form score workflow.</summary>
public static class AdaptiveTaxFormScoreWorkflow
{
    public static AdaptiveTaxFormScoreService NewAdaptiveTaxFormScoreService(IAdaptiveTaxFormScoreStore store, IClock clock)
    {
        return new AdaptiveTaxFormScoreService(
            store,
            AdaptiveTaxFormScoreRules.DefaultAdaptiveTaxFormScoreValidator(),
            clock,
            new List<string>());
    }
}
