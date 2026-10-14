namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves visitor templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record VisitorTemplateService(IVisitorTemplateStore Store, VisitorTemplateValidator Validator, IClock Clock, List<string> Journal, VisitorTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VisitorTemplateStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VisitorTemplateStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor template {code} not found");
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

/// <summary>Functions of visitor template workflow.</summary>
public static class VisitorTemplateWorkflow
{
    public static VisitorTemplateService NewVisitorTemplateService(IVisitorTemplateStore store, IClock clock)
    {
        return new VisitorTemplateService(
            store,
            VisitorTemplateRules.DefaultVisitorTemplateValidator(),
            clock,
            new List<string>(),
            VisitorTemplateEngine.DefaultVisitorTemplateEngine());
    }
}
