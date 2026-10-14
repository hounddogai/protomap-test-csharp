namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves seasonal tax form tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalTaxFormTierWorkflow(ISeasonalTaxFormTierStore Store, SeasonalTaxFormTierValidator Validator, IClock Clock, List<string> Journal, SeasonalTaxFormTierEngine Engine)
{
    public static SeasonalTaxFormTierWorkflow NewSeasonalTaxFormTierWorkflow(ISeasonalTaxFormTierStore store, IClock clock, Registry flags)
    {
        return new SeasonalTaxFormTierWorkflow(
            store,
            SeasonalTaxFormTierRules.DefaultSeasonalTaxFormTierValidator(),
            clock,
            new List<string>(),
            SeasonalTaxFormTierEngine.DefaultSeasonalTaxFormTierEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal tax form tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalTaxFormTierStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal tax form tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalTaxFormTierStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal tax form tier {code} not found");
        }
        var result = Engine.Run(item);
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
