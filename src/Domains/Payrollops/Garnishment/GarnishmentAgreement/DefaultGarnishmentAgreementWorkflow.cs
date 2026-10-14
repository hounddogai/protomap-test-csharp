namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentAgreement;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default garnishment agreements through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultGarnishmentAgreementService(IDefaultGarnishmentAgreementStore Store, DefaultGarnishmentAgreementValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default garnishment agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultGarnishmentAgreementStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default garnishment agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultGarnishmentAgreementStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of default garnishment agreement workflow.</summary>
public static class DefaultGarnishmentAgreementWorkflow
{
    public static DefaultGarnishmentAgreementService NewDefaultGarnishmentAgreementService(IDefaultGarnishmentAgreementStore store, IClock clock)
    {
        return new DefaultGarnishmentAgreementService(
            store,
            DefaultGarnishmentAgreementRules.DefaultDefaultGarnishmentAgreementValidator(),
            clock,
            new List<string>());
    }
}
