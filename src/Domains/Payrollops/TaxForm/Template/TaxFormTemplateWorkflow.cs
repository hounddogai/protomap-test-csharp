namespace Acme.Workforce.Payrollops.TaxForm.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tax form templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record TaxFormTemplateWorkflow(ITaxFormTemplateStore Store, TaxFormTemplateValidator Validator, IClock Clock, List<string> Journal, TaxFormTemplateEngine Engine)
{
    public static TaxFormTemplateWorkflow NewTaxFormTemplateWorkflow(ITaxFormTemplateStore store, IClock clock, Registry flags)
    {
        return new TaxFormTemplateWorkflow(
            store,
            TaxFormTemplateRules.DefaultTaxFormTemplateValidator(),
            clock,
            new List<string>(),
            TaxFormTemplateEngine.DefaultTaxFormTemplateEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TaxFormTemplateStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TaxFormTemplateStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form template {code} not found");
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
