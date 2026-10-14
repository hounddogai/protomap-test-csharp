namespace Acme.Workforce.Payrollops.Garnishment.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves garnishment agreements through their life cycle and keeps a journal of what it did.</summary>
public sealed record GarnishmentAgreementManager(IGarnishmentAgreementStore Store, GarnishmentAgreementValidator Validator, IClock Clock, List<string> Journal, GarnishmentAgreementEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GarnishmentAgreementStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GarnishmentAgreementStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment agreement {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of garnishment agreement workflow.</summary>
public static class GarnishmentAgreementWorkflow
{
    public static GarnishmentAgreementManager NewGarnishmentAgreementManager(IGarnishmentAgreementStore store, IClock clock, Registry flags)
    {
        return new GarnishmentAgreementManager(
            store,
            GarnishmentAgreementRules.DefaultGarnishmentAgreementValidator(),
            clock,
            new List<string>(),
            GarnishmentAgreementEngine.DefaultGarnishmentAgreementEngine(flags));
    }
}
