namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global license seat templates through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record GlobalLicenseSeatTemplateCoordinator(IGlobalLicenseSeatTemplateStore Store, GlobalLicenseSeatTemplateValidator Validator, IClock Clock, List<string> Journal, GlobalLicenseSeatTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global license seat template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalLicenseSeatTemplateStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global license seat template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalLicenseSeatTemplateStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global license seat template {code} not found");
        }
        var result = Engine.Evaluate(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of global license seat template workflow.</summary>
public static class GlobalLicenseSeatTemplateWorkflow
{
    public static GlobalLicenseSeatTemplateCoordinator NewGlobalLicenseSeatTemplateCoordinator(IGlobalLicenseSeatTemplateStore store, IClock clock)
    {
        return new GlobalLicenseSeatTemplateCoordinator(
            store,
            GlobalLicenseSeatTemplateRules.DefaultGlobalLicenseSeatTemplateValidator(),
            clock,
            new List<string>(),
            GlobalLicenseSeatTemplateEngine.DefaultGlobalLicenseSeatTemplateEngine());
    }
}
