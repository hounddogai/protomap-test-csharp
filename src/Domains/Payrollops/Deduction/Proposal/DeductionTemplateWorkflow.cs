namespace Acme.Workforce.Payrollops.Deduction.Proposal;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves deduction templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeductionTemplateDesk(IDeductionTemplateStore Store, DeductionTemplateValidator Validator, IClock Clock, List<string> Journal, DeductionTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeductionTemplateStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeductionTemplateStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction template {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of deduction template workflow.</summary>
public static class DeductionTemplateWorkflow
{
    public static DeductionTemplateDesk NewDeductionTemplateDesk(IDeductionTemplateStore store, IClock clock)
    {
        return new DeductionTemplateDesk(
            store,
            DeductionTemplateRules.DefaultDeductionTemplateValidator(),
            clock,
            new List<string>(),
            DeductionTemplateEngine.DefaultDeductionTemplateEngine());
    }
}
