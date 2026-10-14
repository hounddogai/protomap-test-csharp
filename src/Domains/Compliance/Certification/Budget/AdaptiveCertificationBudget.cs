namespace Acme.Workforce.Compliance.Certification.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive certification budget is in its life cycle.</summary>
public enum AdaptiveCertificationBudgetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One adaptive certification budget, with what the planners need to act on it.</summary>
public sealed class AdaptiveCertificationBudget(string code, string name, double margin, int windowDays, double fte, int endsAt, AdaptiveCertificationBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public int WindowDays { get; set; } = windowDays;
    public double Fte { get; set; } = fte;
    public int EndsAt { get; set; } = endsAt;
    public AdaptiveCertificationBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive certification budget in status draft.</summary>
    public static AdaptiveCertificationBudget NewAdaptiveCertificationBudget(string code, string name)
    {
        return new AdaptiveCertificationBudget(code, name, 0.0, 0, 0.0, 0, AdaptiveCertificationBudgetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveCertificationBudgetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveCertificationBudgetStatus next)
    {
        switch (Status)
        {
            case AdaptiveCertificationBudgetStatus.Draft:
                return next == AdaptiveCertificationBudgetStatus.Active || next == AdaptiveCertificationBudgetStatus.Closed;
            case AdaptiveCertificationBudgetStatus.Pending:
                switch (next)
                {
                    case AdaptiveCertificationBudgetStatus.Suspended:
                        return true;
                }
                return false;
            case AdaptiveCertificationBudgetStatus.Active:
                return next == AdaptiveCertificationBudgetStatus.Closed;
            case AdaptiveCertificationBudgetStatus.Suspended:
                return next == AdaptiveCertificationBudgetStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AdaptiveCertificationBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive certification budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)WindowDays + (double)EndsAt, 2);
    }

    public AdaptiveCertificationBudget Copy()
    {
        return new AdaptiveCertificationBudget(Code, Name, Margin, WindowDays, Fte, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
