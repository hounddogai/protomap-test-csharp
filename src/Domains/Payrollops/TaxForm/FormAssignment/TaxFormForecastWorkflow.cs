namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tax form forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record TaxFormForecastDesk(ITaxFormForecastStore Store, TaxFormForecastValidator Validator, IClock Clock, List<string> Journal, TaxFormForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TaxFormForecastStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TaxFormForecastStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form forecast {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of tax form forecast workflow.</summary>
public static class TaxFormForecastWorkflow
{
    public static TaxFormForecastDesk NewTaxFormForecastDesk(ITaxFormForecastStore store, IClock clock, Registry flags)
    {
        return new TaxFormForecastDesk(
            store,
            TaxFormForecastRules.DefaultTaxFormForecastValidator(),
            clock,
            new List<string>(),
            TaxFormForecastEngine.DefaultTaxFormForecastEngine(flags));
    }
}
