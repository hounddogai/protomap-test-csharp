namespace Acme.Workforce.Absence.Accrual.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual accrual entry is in its life cycle.</summary>
public enum ManualAccrualEntryStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A manual accrual entry. Its code is unique within a tenant.</summary>
public sealed class ManualAccrualEntry(string code, string label, double budget, string skill, double rating, ManualAccrualEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public string Skill { get; set; } = skill;
    public double Rating { get; set; } = rating;
    public ManualAccrualEntryStatus Status { get; set; } = status;

    /// <summary>Starts a manual accrual entry in status open.</summary>
    public static ManualAccrualEntry NewManualAccrualEntry(string code, string label)
    {
        return new ManualAccrualEntry(code, label, 0.0, "", 0.0, ManualAccrualEntryStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualAccrualEntryStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualAccrualEntryStatus next)
    {
        switch (Status)
        {
            case ManualAccrualEntryStatus.Open:
                return next == ManualAccrualEntryStatus.Resolved || next == ManualAccrualEntryStatus.Archived;
            case ManualAccrualEntryStatus.InReview:
                switch (next)
                {
                    case ManualAccrualEntryStatus.Reopened:
                    case ManualAccrualEntryStatus.Archived:
                        return true;
                }
                return false;
            case ManualAccrualEntryStatus.Resolved:
                switch (next)
                {
                    case ManualAccrualEntryStatus.Archived:
                        return true;
                }
                return false;
            case ManualAccrualEntryStatus.Reopened:
                switch (next)
                {
                    case ManualAccrualEntryStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualAccrualEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual accrual entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Budget * Rating, 2);
    }

    public ManualAccrualEntry Copy()
    {
        return new ManualAccrualEntry(Code, Label, Budget, Skill, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
