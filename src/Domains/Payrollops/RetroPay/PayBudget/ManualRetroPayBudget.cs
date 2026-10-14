namespace Acme.Workforce.Payrollops.RetroPay.PayBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual retro pay budget is in its life cycle.</summary>
public enum ManualRetroPayBudgetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A manual retro pay budget. Its code is unique within a tenant.</summary>
public sealed class ManualRetroPayBudget(string code, string label, string locale, string name, string category, string ownerId, int endsAt, ManualRetroPayBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public string OwnerId { get; set; } = ownerId;
    public int EndsAt { get; set; } = endsAt;
    public ManualRetroPayBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a manual retro pay budget in status idle.</summary>
    public static ManualRetroPayBudget NewManualRetroPayBudget(string code, string label)
    {
        return new ManualRetroPayBudget(code, label, "", "", "", "", 0, ManualRetroPayBudgetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualRetroPayBudgetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualRetroPayBudgetStatus next)
    {
        switch (Status)
        {
            case ManualRetroPayBudgetStatus.Idle:
                switch (next)
                {
                    case ManualRetroPayBudgetStatus.Stopped:
                        return true;
                }
                return false;
            case ManualRetroPayBudgetStatus.Warming:
                return next == ManualRetroPayBudgetStatus.Draining || next == ManualRetroPayBudgetStatus.Stopped;
            case ManualRetroPayBudgetStatus.Ready:
                return next == ManualRetroPayBudgetStatus.Draining || next == ManualRetroPayBudgetStatus.Stopped;
            case ManualRetroPayBudgetStatus.Draining:
                switch (next)
                {
                    case ManualRetroPayBudgetStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualRetroPayBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual retro pay budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)EndsAt;
    }

    public ManualRetroPayBudget Copy()
    {
        return new ManualRetroPayBudget(Code, Label, Locale, Name, Category, OwnerId, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
