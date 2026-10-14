namespace Acme.Workforce.Assets.LicenseSeat.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves license seat templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record LicenseSeatTemplateManager(ILicenseSeatTemplateStore Store, LicenseSeatTemplateValidator Validator, IClock Clock, List<string> Journal, LicenseSeatTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LicenseSeatTemplateStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LicenseSeatTemplateStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat template {code} not found");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of license seat template workflow.</summary>
public static class LicenseSeatTemplateWorkflow
{
    public static LicenseSeatTemplateManager NewLicenseSeatTemplateManager(ILicenseSeatTemplateStore store, IClock clock)
    {
        return new LicenseSeatTemplateManager(
            store,
            LicenseSeatTemplateRules.DefaultLicenseSeatTemplateValidator(),
            clock,
            new List<string>(),
            LicenseSeatTemplateEngine.DefaultLicenseSeatTemplateEngine());
    }
}
