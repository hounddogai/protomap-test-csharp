namespace Acme.Workforce.Payrollops.RetroPay.PayBudget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves retro pay templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPayTemplateService(IRetroPayTemplateStore Store, RetroPayTemplateValidator Validator, IClock Clock, List<string> Journal, RetroPayTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPayTemplateStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPayTemplateStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay template {code} not found");
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

/// <summary>Functions of retro pay template workflow.</summary>
public static class RetroPayTemplateWorkflow
{
    public static RetroPayTemplateService NewRetroPayTemplateService(IRetroPayTemplateStore store, IClock clock)
    {
        return new RetroPayTemplateService(
            store,
            RetroPayTemplateRules.DefaultRetroPayTemplateValidator(),
            clock,
            new List<string>(),
            RetroPayTemplateEngine.DefaultRetroPayTemplateEngine());
    }
}
