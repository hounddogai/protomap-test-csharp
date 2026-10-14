namespace Acme.Workforce.Compensation.PayBand.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pay band templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayBandTemplateCoordinator(IPayBandTemplateStore Store, PayBandTemplateValidator Validator, IClock Clock, List<string> Journal, PayBandTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayBandTemplateStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayBandTemplateStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band template {code} not found");
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

/// <summary>Functions of pay band template workflow.</summary>
public static class PayBandTemplateWorkflow
{
    public static PayBandTemplateCoordinator NewPayBandTemplateCoordinator(IPayBandTemplateStore store, IClock clock, Registry flags)
    {
        return new PayBandTemplateCoordinator(
            store,
            PayBandTemplateRules.DefaultPayBandTemplateValidator(),
            clock,
            new List<string>(),
            PayBandTemplateEngine.DefaultPayBandTemplateEngine(flags));
    }
}
