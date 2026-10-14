namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending visitor profile is in its life cycle.</summary>
public enum PendingVisitorProfileStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a pending visitor profile and its current status.</summary>
public sealed class PendingVisitorProfile(string code, string title, double amount, string salaryBand, string skill, int leaveDays, string phone, bool visible, int priority, List<string> tags, PendingVisitorProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public string SalaryBand { get; set; } = salaryBand;
    public string Skill { get; set; } = skill;
    public int LeaveDays { get; set; } = leaveDays;
    public string Phone { get; set; } = phone;
    public bool Visible { get; set; } = visible;
    public int Priority { get; set; } = priority;
    public List<string> Tags { get; set; } = tags;
    public PendingVisitorProfileStatus Status { get; set; } = status;

    /// <summary>Starts a pending visitor profile in status created.</summary>
    public static PendingVisitorProfile NewPendingVisitorProfile(string code, string title)
    {
        return new PendingVisitorProfile(
            code,
            title,
            0.0,
            "",
            "",
            0,
            "",
            false,
            0,
            new List<string>(),
            PendingVisitorProfileStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingVisitorProfileStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingVisitorProfileStatus next)
    {
        switch (Status)
        {
            case PendingVisitorProfileStatus.Created:
                return next == PendingVisitorProfileStatus.Published || next == PendingVisitorProfileStatus.Retired;
            case PendingVisitorProfileStatus.Validated:
                return next == PendingVisitorProfileStatus.Published || next == PendingVisitorProfileStatus.Retired;
            case PendingVisitorProfileStatus.Published:
                return next == PendingVisitorProfileStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PendingVisitorProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending visitor profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pending visitor profile"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)LeaveDays + Amount, 2);
    }

    public PendingVisitorProfile Copy()
    {
        return new PendingVisitorProfile(
            Code,
            Title,
            Amount,
            SalaryBand,
            Skill,
            LeaveDays,
            Phone,
            Visible,
            Priority,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
